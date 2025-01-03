using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace YourExpenses.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        //[Range(typeof(decimal), "0.01", "100000.00", ErrorMessage = "enter decimal value")]
        [RegularExpression(@"^[0-9]{1,6}\.[0-9]{2}$", ErrorMessage = "enter decimal value of format 9.99")]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public bool IsSubscription { get; set; }
        public DateTime? EndOfSubscription { get; set; }

        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public string? UserId { get; set; }
        public virtual IdentityUser? User { get; set; }
    }
}
