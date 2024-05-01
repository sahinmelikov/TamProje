using QrSystem.Models.BaseId;

namespace QrSystem.Models
{
    public class ParentCategory : Base
    {
        public string Name { get; set; }
        public bool Isactive { get; set; }
        public List<Product> Products { get; set; }
        public BigParentCategory? bigParentCategory { get; set; }
        public int? bigParentCategoryId { get; set; }
        public Restorant Restorant { get; set; }
        public int? RestorantId { get; set; }
    }
}
