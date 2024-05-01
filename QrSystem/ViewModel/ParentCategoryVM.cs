using QrSystem.Models;

namespace QrSystem.ViewModel
{
    public class ParentCategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Isactive {  get; set; }
        public List<Product> Products { get; set; }
    }
}
