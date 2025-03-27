using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mini_Amazon_Clone.Data;
using Mini_Amazon_Clone.DTO;
using Mini_Amazon_Clone.Models;
using Mini_Amazon_Clone.Repositories;

namespace Mini_Amazon_Clone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderRepository _orderRepository;
        private readonly AppDbContext _context;


        public OrdersController(OrderRepository orderRepository, AppDbContext context)
        {
            _orderRepository = orderRepository;
            _context = context;
        }

        [HttpGet("get-orders-with-eager-loading")]
        public async Task<IActionResult> GetOrdersWithProductsEagerLoading()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                OrderID = o.OrderID,
                UserID = o.UserID,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDtoForGetOrders
                {
                    OrderItemID = oi.OrderItemID,
                    ProductID = oi.ProductID,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Product = new ProductDtoForGetOrders
                    {
                        ProductID = oi.Product.ProductID,
                        Name = oi.Product.Name,
                        Description = oi.Product.Description,
                        Price = oi.Product.Price,
                        Stock = oi.Product.Stock
                    }
                }).ToList()
            }).ToList();

            return Ok(new
            {
                msg = "data retrieved with eager loading",
                data = orderDtos
            });
        }

        [HttpGet("customer/{userId}")]
        public async Task<IActionResult> GetCustomerOrders(int userId)
        {
            var orders = await _orderRepository.GetCustomerOrdersAsync(userId);
            return Ok(orders);
        }

        [HttpGet("all")]
        [Authorize(Policy = "CanViewOrdersPolicy")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                OrderID = o.OrderID,
                UserID = o.UserID,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDtoForGetOrders
                {
                    OrderItemID = oi.OrderItemID,
                    ProductID = oi.ProductID,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Product = new ProductDtoForGetOrders
                    {
                        ProductID = oi.Product.ProductID,
                        Name = oi.Product.Name,
                        Description = oi.Product.Description,
                        Price = oi.Product.Price,
                        Stock = oi.Product.Stock
                    }
                }).ToList()
            }).ToList();

            return Ok(new
            {
                msg = "Retrieved Orders",
                data = orderDtos
            });
        }

        [HttpPost("{id}/refund")]
        [Authorize(Policy = "CanRefundOrdersPolicy")]
        public async Task<IActionResult> RefundOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "Refunded";
            await _context.SaveChangesAsync();
            return Ok(new { msg = "Order refunded successfully" });
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto orderDto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? throw new InvalidOperationException("UserId not found in token"));

            // Fetch product details from the database based on ProductIDs
            var productIds = orderDto.Items.Select(i => i.ProductID).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductID))
                .ToListAsync();

            // Validate that all requested products exist
            var missingProducts = productIds.Except(products.Select(p => p.ProductID)).ToList();
            if (missingProducts.Any())
            {
                return BadRequest(new { msg = "Invalid product IDs", missingProductIds = missingProducts });
            }

            // Create the order
            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderItems = orderDto.Items.Select(i => new OrderItem
                {
                    ProductID = i.ProductID,
                    Quantity = i.Quantity,
                    Price = products.First(p => p.ProductID == i.ProductID).Price
                }).ToList()
            };

            // Calculate total amount
            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

            // Save to database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Prepare response with calculated prices
            var responseItems = order.OrderItems.Select(oi => new
            {
                ProductID = oi.ProductID,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList();

            return Ok(new
            {
                msg = "Order created successfully",
                orderId = order.OrderID,
                totalAmount = order.TotalAmount,
                items = responseItems
            });
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? throw new InvalidOperationException("UserId not found in token"));

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.UserID == userId);

            if (order == null) return NotFound();

            var orderDto = new OrderDto
            {
                OrderID = order.OrderID,
                UserID = order.UserID,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDtoForGetOrders
                {
                    OrderItemID = oi.OrderItemID,
                    ProductID = oi.ProductID,
                    Quantity = oi.Quantity, 
                    Price = oi.Price,
                    Product = new ProductDtoForGetOrders
                    {
                        ProductID = oi.Product.ProductID,
                        Name = oi.Product.Name,
                        Description = oi.Product.Description,
                        Price = oi.Product.Price,
                        Stock = oi.Product.Stock
                    }
                }).ToList()
            };

            return Ok(orderDto);
        }
    }
}
