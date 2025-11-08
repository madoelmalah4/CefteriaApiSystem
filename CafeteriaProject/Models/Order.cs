using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaProject.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalPrice { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]

        public User User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }

}
