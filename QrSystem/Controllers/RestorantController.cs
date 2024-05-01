using Microsoft.AspNetCore.Mvc;
using QrSystem.DAL;
using QrSystem.ViewModel;

namespace QrSystem.Controllers
{
    public class RestorantController : Controller
    {
        private readonly AppDbContext _context;

        public RestorantController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM
            {
                Restorants = _context.Restorant.ToList(),
            };
            return View(homeVM);
        }
    }
}
