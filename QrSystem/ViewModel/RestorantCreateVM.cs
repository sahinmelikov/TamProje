namespace QrSystem.ViewModel
{
    public class RestorantCreateVM
    {
        public string RestorantName { get; set; }
        public IFormFile RestorantImagePath { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
