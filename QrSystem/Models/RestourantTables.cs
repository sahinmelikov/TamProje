using QrSystem.Models.BaseId;

namespace QrSystem.Models
{
    public class RestourantTables:Base
    {
        public int TableNumber { get; set; }
        public QrCode QrCode { get; set; }
        public int QrCodeId { get; set; }
        public List<Product> Products { get; set; }
        public Ofisant Ofisant { get; set; }
        public int? OfisantId { get; set; }

    }
}
