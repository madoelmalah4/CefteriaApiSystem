using System.Collections.Generic;

namespace CafeteriaProject.Models.DTO_s
{
    public class OrderDto
    {
        public int Id { get; set; }
        public List<OrderItemSelectionDto> OrderItems { get; set; } = new();
    }

    public class OrderItemSelectionDto
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}
