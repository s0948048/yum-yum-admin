using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using yum_admin.Models;
using yum_admin.Models.DataTransferObject;
using yum_admin.Models.ViewModels;

namespace yum_admin.Controllers
{
    public class IngredientsController(YumyumdbContext context, IAntiforgery antiforgery) : Controller
    {
        private readonly YumyumdbContext _context = context;
        private readonly IAntiforgery _antiforgery = antiforgery;



        // GET: Ingredients
        public async Task<IActionResult> Index()
        {
            var yumyumdbContext = from i in _context.Ingredients
                                  select new IngredientInfo
                                  {
                                      id = i.IngredientId,
                                      name = i.IngredientName,
                                      attrId = i.Attribution.IngredAttributeId,
                                      attrName = i.Attribution.IngredAttributeName,
                                      icon = i.IngredientIcon
                                  };

            return View(await yumyumdbContext.ToListAsync());
        }

        
        // GET: Ingredients/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients
                .Include(i => i.Attribution)
                .FirstOrDefaultAsync(m => m.IngredientId == id);
            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        
        // GET: Ingredients/Create
        public IActionResult Create()
        {
            ViewData["AttributionId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId");
            return View();
        }

        
        // POST: Ingredients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        
        // GET: Ingredients/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            ViewData["AttributionId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId", ingredient.AttributionId);
            return View(ingredient);
        }

        
        // POST: Ingredients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("IngredientId,IngredientName,AttributionId,IngredientIcon")] Ingredient ingredient)
        {
            if (id != ingredient.IngredientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ingredient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IngredientExists(ingredient.IngredientId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AttributionId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId", ingredient.AttributionId);
            return View(ingredient);
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
			del_ID = del_ID.Substring(0,del_ID.Length - 1);
            Console.WriteLine(del_ID);
            await _context.SaveChangesAsync();
            return Json(new { success = true, redirectUrl = Url.Action("index", "ingredients"), message = $"已刪除id：{del_ID}" });
		}

        
        private bool IngredientExists(short id)
        {
            return _context.Ingredients.Any(e => e.IngredientId == id);
        }

		
        [HttpGet]
		public IActionResult GetToken()
		{
			// 生成防護令牌
			var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
			return Ok(new { token = tokens.RequestToken });
		}
	}
}
