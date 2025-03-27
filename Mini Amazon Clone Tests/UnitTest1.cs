//using Xunit;
//using Moq;
//using Dapper;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Threading.Tasks;
//using Mini_Amazon_Clone.Models;
//using Mini_Amazon_Clone.Repositories;
//using Microsoft.Extensions.Configuration;

//public class OrderRepositoryTests
//{
//    private readonly Mock<IDbConnection> _dbConnectionMock;
//    private readonly Mock<IConfiguration> _configurationMock;
//    private readonly OrderRepository _orderRepository;

//    public OrderRepositoryTests()
//    {
//        _dbConnectionMock = new Mock<IDbConnection>();
//        _configurationMock = new Mock<IConfiguration>();
//        _configurationMock.Setup(c => c.GetConnectionString("DefaultConnection")).Returns("FakeConnectionString");
//        _orderRepository = new OrderRepository(_configurationMock.Object);
//    }

//    [Fact]
//    public async Task GetCustomerOrdersAsync_ShouldReturnOrders_WhenOrdersExist()
//    {
//        // Arrange
//        int userId = 1;
//        var mockOrders = new List<Order>
//        {
//            new Order
//            {
//                OrderID = 1,
//                UserID = userId,
//                OrderDate = DateTime.Now,
//                TotalAmount = 100,
//                Status = "Pending",
//                OrderItems = new List<OrderItem>()
//            }
//        };

//        _dbConnectionMock.Setup(db => db.QueryAsync<Order, OrderItem, Product, Order>(
//            It.IsAny<string>(),                                    // SQL query
//            It.IsAny<Func<Order, OrderItem, Product, Order>>(),   // Mapping function
//            It.IsAny<object>(),                                   // Parameters
//            null,                                                 // Transaction (explicitly null)
//            true,                                                 // Buffered (default is true)
//            "OrderID"                                             // SplitOn (adjust as needed)
//        )).ReturnsAsync(mockOrders);

//        // Act
//        var result = await _orderRepository.GetCustomerOrdersAsync(userId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Single(result);
//    }

//    [Fact]
//    public async Task AddOrderAsync_ShouldReturnOrderId_WhenOrderIsAdded()
//    {
//        // Arrange
//        var order = new Order
//        {
//            UserID = 1,
//            OrderDate = DateTime.Now,
//            TotalAmount = 150,
//            Status = "Confirmed",
//            OrderItems = new List<OrderItem>()
//        };

//        _dbConnectionMock.Setup(db => db.ExecuteScalarAsync<int>(
//            It.IsAny<string>(),       // SQL query
//            It.IsAny<object>(),       // Parameters
//            null,                     // Transaction (explicitly null)
//            null,                     // CommandTimeout (explicitly null)
//            null                      // CommandType (explicitly null)
//        )).ReturnsAsync(1);

//        // Act
//        var result = await _orderRepository.AddOrderAsync(order);

//        // Assert
//        Assert.Equal(1, result);
//    }
//}



using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Mini_Amazon_Clone.Repositories;
using Mini_Amazon_Clone.Models;


namespace AmazonBackend.Tests.Repositories
{
    public class OrderRepositoryTests
    {
        private readonly Mock<IOrderRepository> _mockRepo;

        public OrderRepositoryTests()
        {
            _mockRepo = new Mock<IOrderRepository>();
        }

        [Fact]
        public async Task GetOrdersByUserId_ShouldReturnOrders_WhenUserExists()
        {
            // Arrange
            int userId = 1;
            var orders = new List<Order>
            {
                new Order { OrderID = 1, UserID = userId, TotalAmount = 100 },
                new Order { OrderID = 2, UserID = userId, TotalAmount = 200 }
            };

            _mockRepo.Setup(repo => repo.GetOrdersByCustomerId(userId)).ReturnsAsync(orders);

            // Act
            var result = await _mockRepo.Object.GetOrdersByCustomerId(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddOrder_ShouldReturnOrderId_WhenOrderIsAdded()
        {
            // Arrange
            var order = new Order { OrderID = 1, UserID = 1, TotalAmount = 150 };
            _mockRepo.Setup(repo => repo.AddOrder(order)).ReturnsAsync(order.OrderID);

            // Act
            var result = await _mockRepo.Object.AddOrder(order);

            // Assert
            Assert.Equal(1, result);
        }
    }
}