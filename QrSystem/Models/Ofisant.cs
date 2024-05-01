using QrSystem.Models.BaseId;

namespace QrSystem.Models
{
    public class Ofisant:Base
    {
        public string Name { get; set; }
        public Restorant Restorant { get; set; }
        public int? RestorantId { get; set; }
        public List<RestourantTables>RestourantTables { get; set; } 
    }
}
