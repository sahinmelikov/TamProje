using QrSystem.Models.BaseId;

namespace QrSystem.Models
{
    public class Product:Base
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public List<RestourantTables> Tables { get; set; }
        public ParentCategory ParentsCategory { get; set; }
        public int ParentsCategoryId { get; set; }
        public Restorant Restorant { get; set; }
        public int? RestorantId { get; set; }    
    }
}
