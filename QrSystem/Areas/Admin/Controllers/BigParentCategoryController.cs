using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QrSystem.DAL;
using QrSystem.Models.Auth;
using QrSystem.Models;
using QrSystem.ViewModel;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace QrSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Moderator")]
    public class BigParentCategoryController : Controller
    {
        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUser> _userManager;
        public BigParentCategoryController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager = null)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            // Kullanıcının bağlı olduğu restoranın ID'sini alın
            int restorantId = GetCurrentUserRestorantId();

            // İstenen restoranın ID'sine sahip olan restoranı ve onun ürünlerini getirin
            var restoran = _context.Restorant
                                   .Include(r => r.BigParentCategories) // Restoranın ürünlerini de getirin
                                   .FirstOrDefault(r => r.Id == restorantId);

            if (restoran == null)
            {
                // Eğer istenen restoran bulunamazsa, hata mesajı gösterin
                ViewBag.ErrorMessage = "No matching restaurant found!";
                return View();
            }

            return View( restoran.BigParentCategories);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();
            BigParentCategory parents = _context.BigParentCategory.Find(id);
            if (parents is null) return NotFound();
            _context.BigParentCategory.Remove(parents);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        // Kullanıcının bağlı olduğu restoranın ID'sini döndüren yardımcı bir metot
        private int GetCurrentUserRestorantId()
        {
            // Kullanıcının kimliğini alın
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının bağlı olduğu restoranın ID'sini veritabanından alın
            var user = _userManager.FindByIdAsync(userId).Result;
            var restorantId = user.RestorantId;

            return restorantId.HasValue ? restorantId.Value : 0; // Varsayılan değer olarak 0 kullanıldı, siz istediğiniz bir değeri verebilirsiniz.
        }

        public IActionResult Create()
        {
            // Kullanıcının bağlı olduğu restoranın ID'sini alın
            int restorantId = GetCurrentUserRestorantId();

            // Restoran ID'si bulunamazsa hata mesajı döndürün veya uygun bir işlem yapın
            if (restorantId == 0)
            {
                // Hata mesajı göstermek için ModelState kullanabilirsiniz
                ModelState.AddModelError(string.Empty, "Restoran bulunamadı.");
                return View();
            }

            // Restoran ID'si bulunduysa, yeni bir QR kodu oluşturmak için kullanabiliriz
            var qrVM = new BigParentCategoryVM { RestoranId = restorantId };
            return View(qrVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(BigParentCategoryVM productVM)
        {
            // Kullanıcının bağlı olduğu restoranın ID'sini alın
            int restorantId = GetCurrentUserRestorantId();
            BigParentCategory p = new BigParentCategory { Name = productVM.Name, IsActive = productVM.Isactive, RestorantId = restorantId };
            _context.BigParentCategory.Add(p);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            int restorantId = GetCurrentUserRestorantId();

            // Restoran ID'si bulunamazsa hata mesajı döndürün veya uygun bir işlem yapın
            if (restorantId == 0)
            {
                // Hata mesajı göstermek için ModelState kullanabilirsiniz
                ModelState.AddModelError(string.Empty, "Restoran bulunamadı.");
                return View();
            }
            if (id == null || id == 0) return BadRequest();
            BigParentCategory bigParent = _context.BigParentCategory.Find(id);
            if (bigParent is null) return NotFound();
            return View(bigParent);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, BigParentCategory bigParent)
        {
            int restorantId = GetCurrentUserRestorantId();
            if (id == null || id == 0 || id != bigParent.Id || bigParent is null) return BadRequest();
            BigParentCategory exist = _context.BigParentCategory.Find(bigParent.Id);
            exist.Name = bigParent.Name;
            exist.IsActive = bigParent.IsActive;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
