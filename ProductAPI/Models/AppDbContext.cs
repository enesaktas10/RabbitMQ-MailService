using Microsoft.EntityFrameworkCore;
using ProductAPI.Models.Entities;

namespace ProductAPI.Models
{
    public class AppDbContext : DbContext
    {
        

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
    }
}
