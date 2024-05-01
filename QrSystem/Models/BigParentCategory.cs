namespace QrSystem.Models
{
    public class BigParentCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ParentCategory>? ParentCategories { get; set; }
        public bool IsActive { get; set; }
        public Restorant Restorant { get; set; }
        public int? RestorantId { get; set; }
    }
}
