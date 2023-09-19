using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WebApplication1.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public required string Description { get; set; }
        public required string Brand { get; set; }
        public required string Currency { get; set; }
        public required decimal Price { get; set; }
        public required int Stock { get; set; }
        public bool IsDeleted { get; set; }
    }
}
