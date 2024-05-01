using QrSystem.Models;

namespace QrSystem.ViewModel
{
    public class HomeVM
    {
        public List<QrCode> QrCode { get; set; }
        public List<Product> Product { get; set; }
        public List<RestourantTables> RestourantTables { get; set; }
        public List<Restorant> Restorants { get; set; }
        public int QrCodeId { get; set; }
        public int ProductId { get; set; }
        public RestourantTables SelectedTable { get; set; }
        public List<BigParentCategory> BigParentCategories { get; set; }
        public List<ParentCategory> ParentsCategory { get; set; }
        public List<Ofisant> Ofisants { get; set; }
        public Restorant CurrentRestoran { get; set; } // Eklendi: QR kodu ile ilişkilendirilen restoran
    }
}
