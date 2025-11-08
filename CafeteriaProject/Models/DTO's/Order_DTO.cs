namespace CafeteriaProject.Models.DTO_s
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }

        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}
