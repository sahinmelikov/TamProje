using QrSystem.Models.BaseId;
using QrSystem.ViewModel;

namespace QrSystem.Models
{
    public class SaxlanilanSifarish:Base
    {
        public int? SifarislerSayi { get; set; }
        public double? ToplamGelir { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string TableName { get; set; }
        public int QrCodeId { get; set; }
        public int ProductId { get; set; }
        public bool IsDeleted { get; set; }
        public string ImagePath { get; set; }
        public int ProductCount { get; set; }
        public string? Comment { get; set; }
        public Restorant Restorant { get; set; }
        public int? RestorantId { get; set; }
        public string? OfisantName { get; set; }
        public bool? ActiveAll { get; set; }
        public bool IsTimeExpired
        {
            get
            {
                // Eğer DateTime değeri null değilse ve şu anki zaman ondan büyükse
                return DateTime < DateTime.UtcNow;
            }
        }


        public List<BasketİtemVM> OrderedProducts { get; set; }
    }
}
