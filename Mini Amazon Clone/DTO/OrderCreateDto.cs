using static Mini_Amazon_Clone.Controllers.OrdersController;

namespace Mini_Amazon_Clone.DTO
{
    public class OrderCreateDto
    {
        public List<OrderItemDto> Items { get; set; }
    }
}
