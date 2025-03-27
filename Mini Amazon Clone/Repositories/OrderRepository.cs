    using Dapper;
    using System.Data.SqlClient;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Mini_Amazon_Clone.Models;
namespace Mini_Amazon_Clone.Repositories
{

    public class OrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var orderDictionary = new Dictionary<int, Order>();

                var orders = await connection.QueryAsync<Order, OrderItem, Product, Order>(
                    sql: @"SELECT 
                        o.OrderID, o.UserID, o.OrderDate, o.TotalAmount, o.Status,
                        oi.OrderItemID, oi.OrderID, oi.ProductID, oi.Quantity, oi.Price,
                        p.ProductID, p.Name, p.Description, p.Price AS ProductPrice, p.Stock
                      FROM Orders o
                      LEFT JOIN OrderItems oi ON o.OrderID = oi.OrderID
                      LEFT JOIN Products p ON oi.ProductID = p.ProductID
                      WHERE o.UserID = @UserId",
                    map: (order, orderItem, product) =>
                    {
                        if (!orderDictionary.TryGetValue(order.OrderID, out var currentOrder))
                        {
                            currentOrder = order;
                            currentOrder.OrderItems = new List<OrderItem>();
                            orderDictionary.Add(order.OrderID, currentOrder);
                        }

                        if (orderItem != null)
                        {
                            orderItem.Product = product;
                            currentOrder.OrderItems.Add(orderItem);
                        }

                        return currentOrder;
                    },
                    param: new { UserId = userId },
                    splitOn: "OrderItemID, ProductID");

                return orderDictionary.Values;
            }
        }
    }
}
