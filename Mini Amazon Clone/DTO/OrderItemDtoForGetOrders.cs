namespace Mini_Amazon_Clone.DTO
{
    public class OrderItemDtoForGetOrders
    {
        public int OrderItemID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public ProductDtoForGetOrders Product { get; set; }
    }

}
