//using Dapper;
//using System.Data.SqlClient;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Mini_Amazon_Clone.Models;
//namespace Mini_Amazon_Clone.Repositories
//{

//    public class OrderRepository
//    {
//        private readonly string _connectionString;

//        public OrderRepository(IConfiguration configuration)
//        {
//            _connectionString = configuration.GetConnectionString("DefaultConnection");
//        }


//        public async Task<int> AddOrderAsync(Order order)
//        {
//            using (var connection = new SqlConnection(_connectionString))
//            {
//                var query = @"
//            INSERT INTO Orders (UserID, OrderDate, TotalAmount, Status) 
//            VALUES (@UserID, @OrderDate, @TotalAmount, @Status);
//            SELECT CAST(SCOPE_IDENTITY() as int);";

//                var orderId = await connection.ExecuteScalarAsync<int>(query, new
//                {
//                    order.UserID,
//                    order.OrderDate,
//                    order.TotalAmount,
//                    order.Status
//                });

//                if (order.OrderItems != null && order.OrderItems.Any())
//                {
//                    var orderItemsQuery = @"
//                INSERT INTO OrderItems (OrderID, ProductID, Quantity, Price) 
//                VALUES (@OrderID, @ProductID, @Quantity, @Price);";

//                    foreach (var item in order.OrderItems)
//                    {
//                        await connection.ExecuteAsync(orderItemsQuery, new
//                        {
//                            OrderID = orderId,
//                            item.ProductID,
//                            item.Quantity,
//                            item.Price
//                        });
//                    }
//                }

//                return orderId;
//            }
//        }

//    }
//}
using Dapper;
using Microsoft.Data.SqlClient;
using Mini_Amazon_Clone.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Mini_Amazon_Clone.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerId(int customerId);
        Task<int> AddOrder(Order order);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<IEnumerable<Order>> GetOrdersByCustomerId(int userId)
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

        //public async Task<IEnumerable<Order>> GetOrdersByCustomerId(int customerId)
        //{
        //    using var connection = new SqlConnection(_connectionString);
        //    string sql = "SELECT * FROM Orders WHERE UserID = @CustomerId";

        //    var orders = await connection.QueryAsync<Order>(sql, new { CustomerId = customerId });
        //    return orders;
        //}

        public async Task<int> AddOrder(Order order)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Insert the order and get the generated ID
                string insertOrderSql = @"
                    INSERT INTO Orders (UserID, OrderDate, TotalAmount)
                    VALUES (@CustomerId, @OrderDate, @TotalAmount);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                int orderId = await connection.ExecuteScalarAsync<int>(
                    insertOrderSql,
                    new { order.UserID, order.OrderDate, order.TotalAmount },
                    transaction
                );

                // Insert order items
                string insertOrderItemSql = @"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price)
                    VALUES (@OrderId, @ProductId, @Quantity, @Price);";

                foreach (var item in order.OrderItems)
                {
                    await connection.ExecuteAsync(
                        insertOrderItemSql,
                        new { OrderId = orderId, item.ProductID, item.Quantity, item.Price },
                        transaction
                    );
                }

                // Commit transaction
                await transaction.CommitAsync();
                return orderId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}