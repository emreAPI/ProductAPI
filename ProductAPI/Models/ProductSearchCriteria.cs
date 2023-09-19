namespace WebApplication1.Models
{
    public class ProductSearchCriteria
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

}
