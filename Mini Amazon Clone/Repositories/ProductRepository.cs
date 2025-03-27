using Dapper;
using Mini_Amazon_Clone.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Mini_Amazon_Clone.Repositories
{

    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var product = await connection.QueryFirstOrDefaultAsync<Product>(
                    sql: @"SELECT 
                        ProductID, Name, Description, Price, Stock, CreatedBy
                      FROM Products
                      WHERE ProductID = @ProductId",
                    param: new { ProductId = productId });

                return product;
            }
        }
    }
}
