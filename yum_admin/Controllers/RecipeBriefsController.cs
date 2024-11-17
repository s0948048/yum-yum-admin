using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using yum_admin.Models;
using yum_admin.Models.ViewModels;

namespace yum_admin.Controllers
{
    public class RecipeBriefsController : Controller
    {
        private readonly YumyumdbContext _context;

        public RecipeBriefsController(YumyumdbContext context)
        {
            _context = context;
        }

        // GET: RecipeBriefs
        public async Task<IActionResult> Index()
        {
            var yumyumdbContext = _context.RecipeBriefs.Include(r => r.Creator).Include(r => r.RecipeClass);
            return View(await yumyumdbContext.ToListAsync());
        }

        // GET: RecipeBriefs/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipeBrief = await _context.RecipeBriefs
                .Include(r => r.Creator)
                .Include(r => r.RecipeClass)
                .FirstOrDefaultAsync(m => m.RecipeId == id);
            if (recipeBrief == null)
            {
                return NotFound();
            }

            return View(recipeBrief);
        }

        // GET: RecipeBriefs/Create
        public IActionResult Create()
        {
            ViewData["CreatorId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId");
            ViewData["RecipeClassId"] = new SelectList(_context.RecipeClasses, "RecipeClassId", "RecipeClassId");
            return View();
        }

        // POST: RecipeBriefs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecipeId,RecipeName,RecipeClassId,FinishMinute,CreatorId,PersonQuantity,CreateDate,ClickTime")] RecipeBrief recipeBrief)
        {
            if (ModelState.IsValid)
            {
                _context.Add(recipeBrief);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatorId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId", recipeBrief.CreatorId);
            ViewData["RecipeClassId"] = new SelectList(_context.RecipeClasses, "RecipeClassId", "RecipeClassId", recipeBrief.RecipeClassId);
            return View(recipeBrief);
        }

        // GET: RecipeBriefs/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipeBrief = await _context.RecipeBriefs.FindAsync(id);
            if (recipeBrief == null)
            {
                return NotFound();
            }
            ViewData["CreatorId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId", recipeBrief.CreatorId);
            ViewData["RecipeClassId"] = new SelectList(_context.RecipeClasses, "RecipeClassId", "RecipeClassId", recipeBrief.RecipeClassId);
            return View(recipeBrief);
        }

        // POST: RecipeBriefs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("RecipeId,RecipeName,RecipeClassId,FinishMinute,CreatorId,PersonQuantity,CreateDate,ClickTime")] RecipeBrief recipeBrief)
        {
            if (id != recipeBrief.RecipeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recipeBrief);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeBriefExists(recipeBrief.RecipeId))
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
            ViewData["CreatorId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId", recipeBrief.CreatorId);
            ViewData["RecipeClassId"] = new SelectList(_context.RecipeClasses, "RecipeClassId", "RecipeClassId", recipeBrief.RecipeClassId);
            return View(recipeBrief);
        }

        // GET: RecipeBriefs/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipeBrief = await _context.RecipeBriefs
                .Include(r => r.Creator)
                .Include(r => r.RecipeClass)
                .FirstOrDefaultAsync(m => m.RecipeId == id);
            if (recipeBrief == null)
            {
                return NotFound();
            }

            return View(recipeBrief);
        }

        // POST: RecipeBriefs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var recipeBrief = await _context.RecipeBriefs.FindAsync(id);
            if (recipeBrief != null)
            {
                _context.RecipeBriefs.Remove(recipeBrief);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecipeBriefExists(short id)
        {
            return _context.RecipeBriefs.Any(e => e.RecipeId == id);
        }
        public IActionResult Recipe()
        {
            // Step 1: 查詢最新的 RecipeRecords 和 RecipeRecordFields，並保持在資料庫內
            var latestRecipeRecords = _context.RecipeRecords
                .GroupBy(r => r.RecipeId)
                .Select(g => new
                {
                    RecipeId = g.Key,
                    LatestRecDate = g.Max(r => r.RecipeRecDate) // 取最大日期/最新日期
                });

            var latestRecipeFields = _context.RecipeRecordFields
                .GroupBy(rf => rf.RecipeId)
                .Select(g => new
                {
                    RecipeId = g.Key,
                    LatestField = g.Max(rf => rf.RecipeField) // 取最大欄位
                });

            // Step 2: 查詢所有需要的數據
            var recipesQuery = from brief in _context.RecipeBriefs
                               join record in _context.RecipeRecords
                                   on brief.RecipeId equals record.RecipeId
                               join field in _context.RecipeRecordFields
                                   on brief.RecipeId equals field.RecipeId
                               join state in _context.RecipeStates
                                   on record.RecipeStatusCode equals state.RecipeStateCode
                               join userInfo in _context.UserSecretInfos
                                   on brief.CreatorId equals userInfo.UserId
                               join latestRecord in latestRecipeRecords
                                   on new { RecipeId = brief.RecipeId, RecipeRecDate = record.RecipeRecDate }
                                      equals new { RecipeId = latestRecord.RecipeId, RecipeRecDate = latestRecord.LatestRecDate }
                               join latestField in latestRecipeFields
                                   on new { RecipeId = brief.RecipeId, RecipeField = field.RecipeField }
                                      equals new { RecipeId = latestField.RecipeId, RecipeField = latestField.LatestField }
                               select new RecipeViewModel
                               {
                                   RecipeStateDescription = state.RecipeStateDescript,
                                   RecipeStateCode = state.RecipeStateCode,
                                   RecipeName = brief.RecipeName,
                                   RecipeId = brief.RecipeId,
                                   UserNickname = userInfo.UserNickname,
                                   RecipeField = field.RecipeField,
                                   RecipeRecDate = record.RecipeRecDate.ToDateTime(TimeOnly.MinValue)
                               };

            // Step 3: 加載查詢結果到內存
            var recipes = recipesQuery.ToList();

            return View(recipes);
        }
        public IActionResult RecipeDetail(int recipeId)
        {
            // 查詢 Recipe 的所有版本
            var recipeVersions = _context.RecipeRecords
                .Where(r => r.RecipeId == recipeId)
                .OrderByDescending(r => r.RecipeRecVersion)
                .ToList(); // 先將結果加載到內存中

            var recipeBrief = _context.RecipeBriefs
                .FirstOrDefault(b => b.RecipeId == recipeId);

            // 檢查數據是否存在
            if (recipeVersions == null || recipeBrief == null)
            {
                return NotFound(); // 若無數據，返回 404
            }

            // 最新版本
            var maxVersionRecord = recipeVersions.FirstOrDefault();

            // 歷史版本（不包括最新版本）
            var previousVersions = recipeVersions.Skip(1).ToList();

            // 查詢 RecipeField 的所有數據
            var recipeFields = _context.RecipeRecordFields
                .Where(rf => rf.RecipeId == recipeId)
                .ToList(); // 同樣加載到內存中

            // 返回給 View 的模型
            var viewModel = new RecipeDetailViewModel
            {
                RecipeId = recipeId,
                RecipeName = recipeBrief.RecipeName,
                MaxVersion = maxVersionRecord?.RecipeRecVersion ?? 0,
                PrevVersions = previousVersions.Select(v => new RecipeVersionDetail
                {
                    RecipeRecVersion = v.RecipeRecVersion,
                    RecipeStatusCode = v.RecipeStatusCode,
                    RecipeRecDate = v.RecipeRecDate.ToDateTime(TimeOnly.MinValue)
                }).ToList(),
                RecipeRecDate = maxVersionRecord?.RecipeRecDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                RecipeFields = recipeFields.Select(f => new RecipeFieldDetail
                {
                    FieldShot = f.FieldShot,
                    FieldDescript = f.FieldDescript,
                    FieldCheck = f.FieldCheck,
                    FieldComment = f.FieldComment
                }).ToList()
            };

            return View(viewModel);
        }
        public IActionResult RecipeInfo()
        {
            return View();
        }
    }
}
