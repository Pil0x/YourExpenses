using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YourExpenses.Data;
using YourExpenses.Helpers;
using YourExpenses.Models;

namespace YourExpenses.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index(int? month, int? year)
        {
            DateHelper.SetViewBagMonthYear(ref month, ref year, ViewBag);

            var userId = User.GetUserId();

            var date = new DateTime(year.Value, month.Value, 1);

            var expenses = await _context.Expense
                .Where(x => x.UserId == userId && x.CreatedAt.Month == month && x.CreatedAt.Year == year ||
                    x.IsSubscription && x.EndOfSubscription.HasValue && x.EndOfSubscription > DateTime.Now && x.CreatedAt < date && x.EndOfSubscription > date)
                .Include(e => e.Category)
                .Include(e => e.User)
                .ToListAsync();

            return View(expenses);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .Include(e => e.Category)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");
            return View();
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CreatedAt,Name,Amount,Description,IsSubscription,EndOfSubscription,CategoryId")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                expense.UserId = User.GetUserId();

                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", expense.CategoryId);
            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense.FindAsync(id);

            if (expense == null || expense.UserId != User.GetUserId())
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", expense.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", expense.UserId);
            return View(expense);
        }

        // POST: Expenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CreatedAt,Name,Amount,Description,IsSubscription,EndOfSubscription,CategoryId,UserId")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            var expenseFromDb = await _context.Expense.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (expenseFromDb == null || expenseFromDb.UserId != User.GetUserId())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    expense.UserId = User.GetUserId();

                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", expense.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", expense.UserId);
            return View(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .Include(e => e.Category)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null || expense.UserId != User.GetUserId())
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expense.FindAsync(id);

            if (expense == null || expense.UserId != User.GetUserId())
            {
                return NotFound();
            }

            if (expense != null)
            {
                _context.Expense.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expense.Any(e => e.Id == id);
        }
    }
}
