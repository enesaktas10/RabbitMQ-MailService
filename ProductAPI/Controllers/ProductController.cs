using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using ProductAPI.Models.Entities;
using ProductAPI.ViewModels;

namespace ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(AppDbContext _context) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductAddVM model)
        {
            var newProduct = new Product()
            {
                FirstName = model.FirstName,
                Mail = model.Mail,
                ProductName = model.ProductName,
                IsPublished = false,
            };

            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();


            Console.WriteLine($"Product created: {newProduct.ProductName} - {newProduct.Mail}");
            return Ok(newProduct);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }
    }
}
