using asp_net_core_Handling_concurrency_web_API.Model;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace asp_net_core_Handling_concurrency_web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;

        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> GetAll()
        {
            using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
            {
                await connection.OpenAsync();
                var products = await connection.QueryAsync<Product>("SELECT * FROM Product");

                return products;
            }
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<Product>> GetById(Guid productId)
        {
            using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
            {
                await connection.OpenAsync();
                var product = await connection.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Product WHERE ProductId = @ProductId", new { ProductId = productId });
                if (product == null) return NotFound();
                return Ok(product);
            }
        }

        [HttpPut()]
        public async Task<ActionResult<Product>> Put([FromBody] Product product)
        {
            using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
            {
                await connection.OpenAsync();
                var existingProduct = await connection.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Product WHERE ProductId = @ProductId", new { ProductId = product.ProductId });
                if (existingProduct == null)
                {
                    return new NotFoundResult();
                }
                if (Convert.ToBase64String(existingProduct.Version) != Convert.ToBase64String(product.Version))
                {
                    return StatusCode(409);
                }
                var rowsUpdated = await connection.ExecuteAsync(@"UPDATE Product
                                                                SET ProductName=@ProductName,
                                                                    UnitPrice=@UnitPrice,
                                                                    UnitsInStock=@UnitsInStock
                                                                WHERE ProductId = @ProductId
                                                                    AND Version = @Version",
                                                                product);
                if (rowsUpdated != 1)
                {
                    return StatusCode(409);
                }
                var savedProduct = await connection.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Product WHERE ProductId = @ProductId", new { ProductId = product.ProductId });
                return Ok(savedProduct);
            }
        }
    }
}
