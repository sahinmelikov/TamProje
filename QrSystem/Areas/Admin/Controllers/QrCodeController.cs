using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrSystem.DAL;
using QrSystem.Models;
using QrSystem.Models.Auth;
using QrSystem.ViewModel;
using System.Security.Claims;

namespace QrSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Moderator")]
    public class QrCodeController: Controller
    {
        private readonly UserManager<AppUser> _userManager;
        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;
        public QrCodeController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            // Kullanıcının bağlı olduğu restoranın ID'sini alın
            int restorantId = GetCurrentUserRestorantId();

            // Kullanıcının bağlı olduğu restorana ait QR kodlarını getirin
            var qrCodes = _context.QrCodes.Include(q => q.Restorant)
                                          .Where(q => q.RestorantId == restorantId)
                                          .ToList();

            return View(qrCodes);
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();
            QrCode qrCode= _context.QrCodes.Find(id);
            if (qrCode is null) return NotFound();
            _context.QrCodes.Remove(qrCode);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
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
            var qrVM = new QrCodeVM { RestorantId = restorantId };
            return View(qrVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(QrCodeVM qrVM)
        {
            // Kullanıcının bağlı olduğu restoranın ID'sini alın
            int restorantId = GetCurrentUserRestorantId();

            // QrCode nesnesini oluştururken RestorantId özelliğine bu restoranı atayın
            QrCode qrCode = new QrCode
            {
                QRCode = qrVM.QrCode,
                DateTime = DateTime.Now,
                RestorantId = restorantId // Kullanıcının bağlı olduğu restoranın ID'sini kullanın
            };

            // QrCode nesnesini veritabanına ekleyin
            _context.QrCodes.Add(qrCode);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id == 0) return BadRequest();
            QrCode qrCode = _context.QrCodes.Find(id);
            if (qrCode is null) return NotFound();
            return View(qrCode);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, QrCode qrCode)
        {
            if (id == null || id == 0 || id != qrCode.Id || qrCode is null) return BadRequest();
            QrCode exist = _context.QrCodes.Find(qrCode.Id);
            exist.QRCode = qrCode.QRCode;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
