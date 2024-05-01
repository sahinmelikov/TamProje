using QrSystem.Models;

namespace QrSystem.ViewModel
{
    public class OfisantVM
    {
        public string Name { get; set; }
        public Restorant Restorant { get; set; }
        public int? RestorantId { get; set; }
        public List<RestourantTables> RestourantTables { get; set; }
    }
}
