using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourExpenses.Data;
using YourExpenses.Helpers;
using YourExpenses.Models;

namespace YourExpenses.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index(int? month, int? year)
        {
            DateHelper.SetViewBagMonthYear(ref month, ref year, ViewBag);
            var date = new DateTime(year!.Value, month!.Value, 1);

            var userId = User.GetUserId();

            var categories = await _context.Category
                .Include(c => c.User)
                .Where(x => x.UserId == null || x.UserId == userId)
                .ToListAsync();

            var monthExpenses = await _context.Expense
                .Where(x => x.UserId == userId && x.CreatedAt.Month == month && x.CreatedAt.Year == year ||
                    x.IsSubscription && x.EndOfSubscription.HasValue && x.EndOfSubscription > DateTime.Now && x.CreatedAt < date && x.EndOfSubscription > date)
                .ToListAsync();

            var monthExpensesSumByCategory = monthExpenses
                .GroupBy(x => x.CategoryId)
                .Select(x => new { CategoryId = x.Key, Sum = x.Sum(x => x.Amount) })
                .ToDictionary(x => x.CategoryId ?? 0, x => x.Sum);

            ViewBag.CategorySum = monthExpensesSumByCategory;


            return View(categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .Include(c => c.User)
                .Where(x => x.UserId == null || x.UserId == User.GetUserId())
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            ViewBag.Expenses = await _context
                .Expense
                .Where(x => x.CategoryId == id && x.UserId == User.GetUserId())
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.UserId = User.GetUserId();

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(id);

            if (category == null || category.UserId == null || category.UserId != User.GetUserId())
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            var userId = User.GetUserId();

            if (id != category.Id || category.UserId == null || category.UserId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.UserId = User.GetUserId();
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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

            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var userId = User.GetUserId();

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null || category.UserId == null || category.UserId != userId)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.GetUserId();

            var category = await _context.Category.FindAsync(id);

            if (category?.UserId == null || category.UserId != userId)
            {
                return NotFound();
            }

            if (category != null)
            {
                _context.Category.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.Id == id);
        }
    }
}
