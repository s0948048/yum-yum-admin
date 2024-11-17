using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using yum_admin.Models;
using yum_admin.Models.DataTransferObject;
using yum_admin.Models.ViewModels;

namespace yum_admin.Controllers
{
	public class IngredientsController(YumyumdbContext context, IAntiforgery antiforgery, IServiceProvider serviceProvider) : Controller
	{
		private readonly YumyumdbContext _context = context;
		private readonly IAntiforgery _antiforgery = antiforgery;
		private readonly IServiceProvider _serviceProvider = serviceProvider;


		// GET: Ingredients
		public async Task<IActionResult> Index()
		{
			ViewBag.Attr = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeName");

			// 右側預設
			List<IngredientInfo> allIngredient = await (from i in _context.Ingredients
														select new IngredientInfo
														{
															id = i.IngredientId,
															name = i.IngredientName,
															attrId = i.Attribution.IngredAttributeId,
															attrName = i.Attribution.IngredAttributeName,
															icon = i.IngredientIcon
														}).ToListAsync();

			// 左側預設
			List<IngredientInfo> otherIngredient = await (from i in _context.Ingredients
														  where i.AttributionId == 9
														  select new IngredientInfo
														  {
															  id = i.IngredientId,
															  name = i.IngredientName,
															  attrId = i.Attribution.IngredAttributeId,
															  attrName = i.Attribution.IngredAttributeName,
															  icon = i.IngredientIcon
														  }).ToListAsync();

			List<List<IngredientInfo>> viewModel = new List<List<IngredientInfo>>
			{
				otherIngredient,allIngredient
			};

			return View(viewModel);
		}

		// POST: AJAX 動態篩選右邊食材。回傳PartialView。
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> FoodResult([FromBody] FoodFilter f)
		{
			// 驗證類別
			if (!ModelState.IsValid)
			{
				return new BadRequestObjectResult(new { success = false, message = "查詢無效" });
			}

			// 檢查是否有傳入
			if (f is not null)
			{
				// 看看都是誰來ㄌ
				Console.WriteLine($"{(f.attrId is null ? "none" : f.attrId)}：{(f.name is null ? "none" : f.name)}");
			}

			var ingredients = from i in _context.Ingredients
							  where (
										f == null ||
										(
											(f.name == null || i.IngredientName.Contains(f.name)) &&
											(f.attrId == 0 || i.AttributionId == f.attrId)
										)
									)
							  select new IngredientInfo
							  {
								  id = i.IngredientId,
								  name = i.IngredientName,
								  attrId = i.Attribution.IngredAttributeId,
								  attrName = i.Attribution.IngredAttributeName,
								  icon = i.IngredientIcon
							  };

			return PartialView("_PartialView_FilterFood", await ingredients.ToListAsync());
		}



		// GET: Ingredients/Create
		public IActionResult Create()
		{
			ViewData["AttributionId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId");
			return View();
		}

		// POST: Ingredients/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(IngredientDto i)
		{
			if (ModelState.IsValid)
			{
				Ingredient ingredient = new Ingredient
				{
					IngredientId = i.id,
					IngredientName = i.name,
					AttributionId = i.attrId,
					IngredientIcon = i.icon
				};

				_context.Add(ingredient);
				await _context.SaveChangesAsync();
				return RedirectToAction("Index");
			}

			ViewData["AttributionId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId", i.attrId);
			return View(i);
		}


		// POST: Ingredients/Delete/5
		[HttpPost, ActionName("Replace")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeFood([FromBody] FoodReplaceDto f)
		{
			string replaceID = "";
			if (f is null)
			{
				return new BadRequestObjectResult(new { success = false, message = "並未正確傳入資料" });
			}
			if (f.originFood is null)
			{
				return new BadRequestObjectResult(new { success = false, message = "請傳入有效的來源食材" });
			}
			if (f.afterFood == 0)
			{
				return new BadRequestObjectResult(new { success = false, message = "請傳入有效的結果食材" });
			}

			List<RecipeIngredient> newRecipeIngredients = new List<RecipeIngredient>();
			List<Task> newTasks = new List<Task>();
			// 替換邏輯：只有兩個功能區可以自己建立食材，因此只須操作兩個區域。

			newTasks.Add(Task.Run(async () =>
			{
				// 1. 把食譜中的食材替換
				//var ingredientRecord = await (from res in _context.RecipeIngredients
				//							  where f.originFood.Contains(res.IngredientId)
				//							  select res).ToListAsync();
				var ingredientRecord = await _context.RecipeIngredients
										.AsNoTracking()
										.Where(res => f.originFood.Contains(res.IngredientId))
										.ToListAsync();

				// 2. 把冰箱中的食材替換
				var ingredientRefrig = await (from res in _context.RefrigeratorStores
											  where f.originFood.Contains(res.IngredientId)
											  select res).ToListAsync();

				using (var scope = _serviceProvider.CreateScope())
				{
					var newContext = scope.ServiceProvider.GetRequiredService<YumyumdbContext>();


					// 因為是複合鍵，所以我們選擇的方式是先新增，再刪除。
					// ver.2 修正：因為add 下一次就是一次指令，因此換成LIST 最後再加。
					foreach (RecipeIngredient r in ingredientRecord)
					{
						newRecipeIngredients.Add(new RecipeIngredient
						{
							RecipeId = r.RecipeId,
							UnitId = r.UnitId,
							Quantity = r.Quantity,
							IngredientId = f.afterFood
						});

						var trackedEntity = _context.RecipeIngredients.Local.FirstOrDefault(e => e.RecipeId == r.RecipeId && e.IngredientId == r.IngredientId);
						if (trackedEntity != null)
						{
							_context.Entry(trackedEntity).State = EntityState.Detached;
						}
						newContext.RecipeIngredients.Remove(r);
					}
					await newContext.SaveChangesAsync();
				}
						

				using (var scope = _serviceProvider.CreateScope())
				{
					// 冰箱就直接修改即可。
					foreach (RefrigeratorStore r in ingredientRefrig)
					{
						r.IngredientId = f.afterFood;
						_context.RefrigeratorStores.Update(r);
					}
					await _context.SaveChangesAsync();
				}

			}));

			await Task.WhenAll(newTasks);

			_context.ChangeTracker.Clear();
			await _context.RecipeIngredients.AddRangeAsync(newRecipeIngredients);
			await _context.SaveChangesAsync();
			replaceID = replaceID.Substring(0, replaceID.Length - 1);
			Console.WriteLine(replaceID);
			Console.WriteLine(f.afterFood);
			return Json(new { success = true, redirectUrl = Url.Action("index", "ingredients"), message = $"已替換id：{replaceID} 結果id：{f.afterFood}" });
		}




		// GET: Ingredients/Delete/5
		public async Task<IActionResult> Delete(short? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var ingredient = from i in _context.Ingredients
							 where i.IngredientId == id
							 select new IngredientInfo
							 {
								 id = i.IngredientId,
								 name = i.IngredientName,
								 attrId = i.Attribution.IngredAttributeId,
								 attrName = i.Attribution.IngredAttributeName,
								 icon = i.IngredientIcon
							 };


			if (ingredient == null)
			{
				return NotFound();
			}

			return View(await ingredient.FirstAsync());
		}


		// POST: Ingredients/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete([FromBody] int[] ids)
		{
			string del_ID = "";

			// 如果POST近來是沒咚咚，就彈回錯誤。
			if (ids.Length == 0)
			{
				return new BadRequestObjectResult(new { success = false, message = "無效的 ID 列表" });
			}

			// 因為是陣列，特別處理他
			foreach (int id in ids)
			{
				Console.WriteLine(id);
				var ingredient = await _context.Ingredients.FindAsync(Convert.ToInt16(id));
				if (ingredient != null)
				{
					_context.Ingredients.Remove(ingredient);
					del_ID += $"{id},";
				}
			}
			del_ID = del_ID.Substring(0, del_ID.Length - 1);
			Console.WriteLine(del_ID);
			await _context.SaveChangesAsync();
			return Json(new { success = true, redirectUrl = Url.Action("index", "ingredients"), message = $"已刪除id：{del_ID}" });
		}




		// token
		// 如果有 [ValidateAntiForgeryToken] 
		// 必須先申請token，也是透過AJAX取得。
		[HttpGet]
		public IActionResult GetToken()
		{
			// 生成防護令牌
			var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
			return Ok(new { token = tokens.RequestToken });
		}




		// 【  Details  】
		// GET: Ingredients/Details/5
		//public async Task<IActionResult> Details(short? id)
		//{
		//	if (id == null)
		//	{
		//		return NotFound();
		//	}

		//	var ingredient = await _context.Ingredients
		//		.Include(i => i.Attribution)
		//		.FirstOrDefaultAsync(m => m.IngredientId == id);
		//	if (ingredient == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(ingredient);
		//}


		//private bool IngredientExists(short id)
		//{
		//	return _context.Ingredients.Any(e => e.IngredientId == id);
		//}


		// 【  Edit  】
		// POST: Ingredients/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> Edit(short id, [Bind("IngredientId,IngredientName,AttributionId,IngredientIcon")] Ingredient ingredient)
		//{
		//	if (id != ingredient.IngredientId)
		//	{
		//		return NotFound();
		//	}

		//	if (ModelState.IsValid)
		//	{
		//		try
		//		{
		//			_context.Update(ingredient);
		//			await _context.SaveChangesAsync();
		//		}
		//		catch (DbUpdateConcurrencyException)
		//		{
		//			if (!IngredientExists(ingredient.IngredientId))
		//			{
		//				return NotFound();
		//			}
		//			else
		//			{
		//				throw;
		//			}
		//		}
		//		return RedirectToAction(nameof(Index));
		//	}
		//	ViewData["AttributionId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId", ingredient.AttributionId);
		//	return View(ingredient);
		//}


		// 【  Edit  】
		// GET: Ingredients/Edit/5
		//public async Task<IActionResult> Edit(short? id)
		//{
		//	if (id == null)
		//	{
		//		return NotFound();
		//	}

		//	var ingredient = await _context.Ingredients.FindAsync(id);
		//	if (ingredient == null)
		//	{
		//		return NotFound();
		//	}
		//	ViewData["AttributionId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId", ingredient.AttributionId);
		//	return View(ingredient);
		//}
	}
}
