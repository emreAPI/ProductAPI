using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace product_client
{
    internal class ProductDto
    {
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
