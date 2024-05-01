namespace QrSystem.Models
{
    public class Restorant
    {
        public int Id { get; set; }
        public string RestorantName { get; set; }
        public string RestorantImagePath { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<QrCode>? QrCodes { get; set; }
        public List<Product>? Products { get; set; }
        public List<ParentCategory> ParentCategories { get; set; }
        public List<BigParentCategory> BigParentCategories { get; set; }
        public List<Ofisant> Ofisants { get; set; }

    }
}
