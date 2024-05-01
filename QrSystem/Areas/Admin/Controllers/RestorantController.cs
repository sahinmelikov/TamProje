using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QrSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Moderator")]
    public class RestorantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
