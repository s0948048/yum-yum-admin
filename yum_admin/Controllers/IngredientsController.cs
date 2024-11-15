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
    public class IngredientsController : Controller
    {
        private readonly YumyumdbContext _context;

        public IngredientsController(YumyumdbContext context)
        {
            _context = context;
        }

        // GET: Ingredients
        public async Task<IActionResult> Index()
        {
            var yumyumdbContext = from i in _context.Ingredients
                                  join ia in _context.IngredAttributes on i.AttributionId equals ia.IngredAttributeId
                                  select new IngredientInfo
                                  {
                                      id = i.IngredientId,
                                      name = i.IngredientName,
                                      attrId = ia.IngredAttributeId,
                                      attrName = ia.IngredAttributeName,
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
        public async Task<IActionResult> Create([Bind("IngredientId,IngredientName,AttributionId,IngredientIcon")] Ingredient ingredient)
        {
            ingredient.Attribution = _context.IngredAttributes.Find(ingredient.AttributionId)!;
            if (!ModelState.IsValid)
            {
                ViewData["AttributionId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId", ingredient.AttributionId);
                return View(ingredient);
            }

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Property: {state.Key}, Error: {error.ErrorMessage}");

                }
            }
            
                _context.Add(ingredient);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
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
                             join ia in _context.IngredAttributes on i.AttributionId equals ia.IngredAttributeId
                             select new IngredientInfo
                             {
                                 id = i.IngredientId,
                                 name = i.IngredientName,
                                 attrId = ia.IngredAttributeId,
                                 attrName = ia.IngredAttributeName,
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
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IngredientExists(short id)
        {
            return _context.Ingredients.Any(e => e.IngredientId == id);
        }
    }
}
