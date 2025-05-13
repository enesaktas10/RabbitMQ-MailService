using System.ComponentModel.DataAnnotations;

namespace ProductAPI.Models.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Mail { get; set; }
        public string ProductName { get; set; }
        public bool IsPublished { get; set; } = false;
    }
}
