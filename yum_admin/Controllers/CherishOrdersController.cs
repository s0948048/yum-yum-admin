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
    public class CherishOrdersController : Controller
    {
        private readonly YumyumdbContext _context;

        public CherishOrdersController(YumyumdbContext context)
        {
            _context = context;
        }

        // GET: CherishOrders
        public async Task<IActionResult> Index()
        {
            var yumyumdbContext = _context.CherishOrders.Include(c => c.GiverUser).Include(c => c.IngredAttribute).Include(c => c.Ingredient).Include(c => c.TradeStateCodeNavigation);
            return View(await yumyumdbContext.ToListAsync());
        }

        // GET: CherishOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cherishOrder = await _context.CherishOrders
                .Include(c => c.GiverUser)
                .Include(c => c.IngredAttribute)
                .Include(c => c.Ingredient)
                .Include(c => c.TradeStateCodeNavigation)
                .FirstOrDefaultAsync(m => m.CherishId == id);
            if (cherishOrder == null)
            {
                return NotFound();
            }

            return View(cherishOrder);
        }

        // GET: CherishOrders/Create
        public IActionResult Create()
        {
            ViewData["GiverUserId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId");
            ViewData["IngredAttributeId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId");
            ViewData["IngredientId"] = new SelectList(_context.Ingredients, "IngredientId", "IngredientId");
            ViewData["TradeStateCode"] = new SelectList(_context.CherishTradeStates, "TradeStateCode", "TradeStateCode");
            return View();
        }

        // POST: CherishOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CherishId,GiverUserId,EndDate,IngredAttributeId,IngredientId,Quantity,ObtainSource,ObtainDate,SubmitDate,ReserveDate,TradeStateCode")] CherishOrder cherishOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cherishOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GiverUserId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId", cherishOrder.GiverUserId);
            ViewData["IngredAttributeId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId", cherishOrder.IngredAttributeId);
            ViewData["IngredientId"] = new SelectList(_context.Ingredients, "IngredientId", "IngredientId", cherishOrder.IngredientId);
            ViewData["TradeStateCode"] = new SelectList(_context.CherishTradeStates, "TradeStateCode", "TradeStateCode", cherishOrder.TradeStateCode);
            return View(cherishOrder);
        }

        // GET: CherishOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cherishOrder = await _context.CherishOrders.FindAsync(id);
            if (cherishOrder == null)
            {
                return NotFound();
            }
            ViewData["GiverUserId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId", cherishOrder.GiverUserId);
            ViewData["IngredAttributeId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId", cherishOrder.IngredAttributeId);
            ViewData["IngredientId"] = new SelectList(_context.Ingredients, "IngredientId", "IngredientId", cherishOrder.IngredientId);
            ViewData["TradeStateCode"] = new SelectList(_context.CherishTradeStates, "TradeStateCode", "TradeStateCode", cherishOrder.TradeStateCode);
            return View(cherishOrder);
        }

        // POST: CherishOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CherishId,GiverUserId,EndDate,IngredAttributeId,IngredientId,Quantity,ObtainSource,ObtainDate,SubmitDate,ReserveDate,TradeStateCode")] CherishOrder cherishOrder)
        {
            if (id != cherishOrder.CherishId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cherishOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CherishOrderExists(cherishOrder.CherishId))
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
            ViewData["GiverUserId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId", cherishOrder.GiverUserId);
            ViewData["IngredAttributeId"] = new SelectList(_context.IngredAttributes, "IngredAttributeId", "IngredAttributeId", cherishOrder.IngredAttributeId);
            ViewData["IngredientId"] = new SelectList(_context.Ingredients, "IngredientId", "IngredientId", cherishOrder.IngredientId);
            ViewData["TradeStateCode"] = new SelectList(_context.CherishTradeStates, "TradeStateCode", "TradeStateCode", cherishOrder.TradeStateCode);
            return View(cherishOrder);
        }

        // GET: CherishOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cherishOrder = await _context.CherishOrders
                .Include(c => c.GiverUser)
                .Include(c => c.IngredAttribute)
                .Include(c => c.Ingredient)
                .Include(c => c.TradeStateCodeNavigation)
                .FirstOrDefaultAsync(m => m.CherishId == id);
            if (cherishOrder == null)
            {
                return NotFound();
            }

            return View(cherishOrder);
        }

        // POST: CherishOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cherishOrder = await _context.CherishOrders.FindAsync(id);
            if (cherishOrder != null)
            {
                _context.CherishOrders.Remove(cherishOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CherishOrderExists(int id)
        {
            return _context.CherishOrders.Any(e => e.CherishId == id);
        }
    }
}
