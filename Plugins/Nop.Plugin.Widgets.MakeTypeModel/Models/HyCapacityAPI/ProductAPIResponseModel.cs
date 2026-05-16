namespace Nop.Plugin.Widgets.MakeTypeModel.Models.HyCapacityAPI
{
    /// <summary>
    /// Represents an product API response model
    /// </summary>
    public class ProductAPIResponseModel
    {
        public bool IsSuccessStatusCode { get; set; }
        public Pager Pager { get; set; }
        public List<Payload> Payload { get; set; }
    }
    public class Payload
    {
        public string HyCapStockNumber { get; set; }
        public string ProductName { get; set; }
        public string ProductNotes { get; set; }
        public decimal SuggListPrice { get; set; }
        public string ProductImage { get; set; }
        public string XRefNumbers { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public List<ProductSpec> ProductSpecs { get; set; }
        public List<Fit> Fits { get; set; }
        public List<CategoryDto> Categories { get; set; }
    }
    public class CategoryDto
    {
        public int CategoryLevel { get; set; }
        public string CategoryName { get; set; }
    }
    public class ProductSpec
    {
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }
    public class Fit
    {
        public string Make { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string FitNote { get; set; }
    }
    public class Pager
    {
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
