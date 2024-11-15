using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using yum_admin.Models;

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
            
            return View();
        }
        public IActionResult RecipeInfo()
        {
            return View();
        }
    }
}
