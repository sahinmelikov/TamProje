using QrSystem.Models.BaseId;

namespace QrSystem.Models
{
    public class Hesabat:Base
    {
        public int? SifarislerSayi { get; set; }
        public double? ToplamGelir { get; set; } 
        public double OfisantSayi { get; set; }
        public int RestorantId { get; set; }
        public Restorant Restorant { get; set; }

    }
}
