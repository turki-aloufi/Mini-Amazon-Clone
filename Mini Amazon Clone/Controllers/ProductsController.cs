using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class ProductsController : ControllerBase
    {
        private readonly ProductRepository _productRepository;
        private readonly AppDbContext _context;

        public ProductsController(ProductRepository productRepository, AppDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(int productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return NotFound();
            return Ok(new
            {
                msg = "Founded thid productProduct",
                data = product
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(new
            {
                msg = "Products retrieved",
                data = products
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                CreatedBy = int.Parse(User.FindFirst("UserId")?.Value ?? throw new InvalidOperationException("UserId not found in token"))
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok(new { msg = "Product added successfully", productId = product.ProductID });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto updatedProduct)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;
            await _context.SaveChangesAsync();
            return Ok(new { msg = "Product updated successfully" });
        }

    }
}
