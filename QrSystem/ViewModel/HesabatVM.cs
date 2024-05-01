using QrSystem.Models;

namespace QrSystem.ViewModel
{
    public class HesabatVM
    {
        public int? SifarislerSayi { get; set; }
        public double? ToplamGelir { get; set; }
        public int RestoranId { get; set; }
        public Restorant Restorant { get; set; }
    }
}
