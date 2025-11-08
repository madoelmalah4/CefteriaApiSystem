namespace CafeteriaProject.Models.DTO_s
{
    public class OrderItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
