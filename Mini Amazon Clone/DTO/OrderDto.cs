namespace Mini_Amazon_Clone.DTO
{
    public class OrderDto
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItemDtoForGetOrders> OrderItems { get; set; }
    }
}
