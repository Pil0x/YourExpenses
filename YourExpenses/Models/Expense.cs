using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace YourExpenses.Models
{
    public class Expense
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Created at")]
        public DateTime CreatedAt { get; set; }
        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Amount")]
        [Required(ErrorMessage = "Amount is required")]
        //[Range(typeof(decimal), "0.01", "100000.00", ErrorMessage = "enter decimal value")]
        [RegularExpression(@"^[0-9]{1,6}\.[0-9]{2}$", ErrorMessage = "enter decimal value of format 9.99")]
        public decimal Amount { get; set; }
        [DisplayName("Description")]
        public string? Description { get; set; }
        [DisplayName("Is subscription")]
        public bool IsSubscription { get; set; }
        [DisplayName("End of subscription")]
        public DateTime? EndOfSubscription { get; set; }

        [DisplayName("Category")]
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public string? UserId { get; set; }
        public virtual IdentityUser? User { get; set; }
    }
}
