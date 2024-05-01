using QrSystem.Models;

namespace QrSystem.ViewModel
{
    public class ProductUpdateVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public IFormFile ImageFile { get; set; }
        public List<RestourantTables> Tables { get; set; }
        public int ParentsCategoryId { get; set; }
    }
}
