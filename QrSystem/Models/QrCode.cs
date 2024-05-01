using QrSystem.Models.BaseId;

namespace QrSystem.Models
{
    public class QrCode:Base
    {
        public string QRCode { get;set; }
       public Restorant Restorant { get;set; }
        public int? RestorantId { get; set; }
      
      
    }
}
