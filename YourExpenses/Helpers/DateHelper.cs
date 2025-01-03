namespace YourExpenses.Helpers
{
    public static class DateHelper
    {
        public static void SetViewBagMonthYear(ref int? month, ref int? year, dynamic viewBag)
        {
            month ??= DateTime.Now.Month;
            year ??= DateTime.Now.Year;

            viewBag.Month = month;
            viewBag.Year = year;

            viewBag.NextMonth = month == 12 ? 1 : month + 1;
            viewBag.NextYear = month == 12 ? year + 1 : year;

            viewBag.PreviousMonth = month == 1 ? 12 : month - 1;
            viewBag.PreviousYear = month == 1 ? year - 1 : year;
        }
    }
}
