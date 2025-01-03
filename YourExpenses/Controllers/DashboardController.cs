using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using YourExpenses.Data;
using YourExpenses.Helpers;
using YourExpenses.Models;

namespace YourExpenses.Controllers
{
    [Authorize]
    public class DashboardController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IActionResult> Index(int? year)
        {
            year ??= DateTime.Now.Year;

            ViewBag.Year = year;
            ViewBag.NextYear = year + 1;
            ViewBag.PreviousYear = year - 1;

            var userId = User.GetUserId();
            var firstDayOfYear = new DateTime(year.Value, 1, 1);
            var lastDayOfYear = new DateTime(year.Value, 12, 31);

            var yearExpenses = await _context.Expense
                .Include(e => e.Category)
                .Where(x => x.UserId == userId)
                .Where(x => x.IsSubscription ?
                    x.EndOfSubscription.HasValue &&
                        x.CreatedAt <= lastDayOfYear && x.EndOfSubscription >= firstDayOfYear

                    : x.CreatedAt >= firstDayOfYear && x.CreatedAt <= lastDayOfYear)
                .ToListAsync();

            var monthExpensesSumByCategory = new List<ChartModel>();
            var allExpensesPerMonth = new List<Expense>();

            for (int i = 1; i <= 12; i++)
            {
                var firstDayOfMonth = new DateTime(year.Value, i, 1);
                var lastDayOfMonth = new DateTime(year.Value, i, DateTime.DaysInMonth(year.Value, i));

                var expensesPerMonth = yearExpenses
                    .Where(x => !x.IsSubscription && x.CreatedAt.Month == i || IsSubscriptionInMonth(x, i, year.Value))
                    .ToList();

                allExpensesPerMonth.AddRange(expensesPerMonth);

                var monthExpensesSum = expensesPerMonth
                    .Sum(x => x.Amount);

                monthExpensesSumByCategory.Add(new ChartModel
                {
                    Id = i,
                    Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
                    Amount = monthExpensesSum
                });
            }

            ViewBag.CategoryChart = GetCategoryChart(allExpensesPerMonth);

            return View(monthExpensesSumByCategory);
        }

        private bool IsSubscriptionInMonth(Expense expense, int month, int year)
        {
            if (!expense.IsSubscription)
                return false;

            if (!expense.EndOfSubscription.HasValue)
                return false;

            if (expense.CreatedAt.Year == year && expense.CreatedAt.Month == month)
                return true;

            if (expense.CreatedAt <= new DateTime(year, month, 1) &&
                expense.EndOfSubscription.Value.Year >= year && expense.EndOfSubscription.Value.Month >= month)
                return true;

            return false;
        }

        private Dictionary<string, decimal> GetCategoryChart(List<Expense> expenses)
        {
            return expenses
                .GroupBy(x => x.Category?.Name)
                .Select(x => new { CategoryName = x.Key, Sum = x.Sum(x => x.Amount) })
                .ToDictionary(x => x.CategoryName ?? "Inne", x => x.Sum);
        }
    }
}
