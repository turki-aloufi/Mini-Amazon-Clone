using Microsoft.EntityFrameworkCore;
using Mini_Amazon_Clone.Data;
using Mini_Amazon_Clone.Models;

namespace Mini_Amazon_Clone.Services
{
    public class OrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public List<Order> GetOrdersWithProducts()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToList();

            return orders;
        }
    }
}
