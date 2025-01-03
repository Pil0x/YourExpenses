using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace YourExpenses.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }


        public virtual ICollection<Expense>? Expenses { get; set; }

        public string? UserId { get; set; }
        public virtual IdentityUser? User { get; set; }
    }
}
