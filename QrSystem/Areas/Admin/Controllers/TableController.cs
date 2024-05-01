using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QrSystem.DAL;
using QrSystem.Migrations;
using QrSystem.Models;
using QrSystem.Models.Auth;
using QrSystem.ViewModel;
using System.Security.Claims;

namespace QrSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Moderator")]
    public class TableController:Controller
    {
        private readonly UserManager<AppUser> _userManager;

        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;
        public TableController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            // Kullanıcının bağlı olduğu restoranın ID'sini alın
            int restorantId = GetCurrentUserRestorantId();

            // Kullanıcının bağlı olduğu restorana ait masaları getirin
            var tables = _context.Tables.Include(d=>d.QrCode.Restorant).Include(d=>d.Ofisant).Where(t => t.QrCode.RestorantId == restorantId&& t.Ofisant.RestorantId==restorantId).ToList();

            return View(tables);
        }

        // Kullanıcının bağlı olduğu restoranın ID'sini döndüren yardımcı bir metot
        
        public IActionResult Create()
        {
            int restorantId = GetCurrentUserRestorantId();
            List<QrCode> qrCodes = _context.QrCodes.ToList();
            List<SelectListItem> parentiTEMs = qrCodes.Where(p => p.RestorantId == restorantId).Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text=p.QRCode.ToString()
            }).ToList();

            ViewBag.QrCode = parentiTEMs;
            List<Ofisant> ofisants= _context.Ofisant.Where(p => p.RestorantId == restorantId).ToList();
            List<SelectListItem> Select = ofisants.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            ViewBag.Ofisants = Select;
            // Kullanıcının bağlı olduğu restoranın ID'sini alın

            // Restoran ID'si bulunamazsa hata mesajı döndürün veya uygun bir işlem yapın
            if (restorantId == 0)
            {
                // Hata mesajı göstermek için ModelState kullanabilirsiniz
                ModelState.AddModelError(string.Empty, "Restoran bulunamadı.");
                return View();
            }

            // Restoran ID'si bulunduysa, yeni bir QR kodu oluşturmak için kullanabiliriz
            var qrVM = new TableVM { RestorantId = restorantId };
            return View(qrVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(TableVM tableVM)
        {
            // Kullanıcının bağlı olduğu restoranın ID'sini alın
            int restorantId = GetCurrentUserRestorantId();
            
            RestourantTables tables = new RestourantTables { OfisantId=tableVM.OfisantId,QrCodeId = tableVM.QrCodeId, TableNumber = tableVM.TableNumber, DateTime = DateTime.Now  };
            _context.Tables.Add(tables);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            int restorantId = GetCurrentUserRestorantId();
            List<QrCode> qrCodes = _context.QrCodes.ToList();
            List<SelectListItem> BigparentiTEMs = qrCodes.Where(p => p.RestorantId == restorantId).Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.QRCode.ToString()
            }).ToList();

            ViewBag.QrCode = BigparentiTEMs;
            List<Ofisant> ofisants= _context.Ofisant.ToList();
            List<SelectListItem> Select = ofisants.Where(p => p.RestorantId == restorantId).Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            ViewBag.Ofisants = Select;
            if (id == null || id == 0) return BadRequest();
            RestourantTables table = _context.Tables.Find(id);
            if (table is null) return NotFound();
            return View(table);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, RestourantTables table)
        {
            int restorantId = GetCurrentUserRestorantId();
            List<QrCode> qrCodes = _context.QrCodes.ToList();
            List<SelectListItem> BigparentiTEMs = qrCodes.Where(p => p.RestorantId == restorantId).Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.QRCode.ToString()
            }).ToList();
            ViewBag.QrCode = BigparentiTEMs;
            List<Ofisant> ofisants = _context.Ofisant.ToList();
            List<SelectListItem> Select = ofisants.Where(p => p.RestorantId == restorantId).Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            ViewBag.Ofisants = Select;
            if (id == null || id == 0 || id != table.Id || table is null) return BadRequest();
            RestourantTables exist = _context.Tables.Find(table.Id);
            exist.TableNumber = table.TableNumber;
            exist.QrCode = table.QrCode;
            exist.OfisantId = table.OfisantId;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();
            RestourantTables tables= _context.Tables.Find(id);
            if (tables is null) return NotFound();
            _context.Tables.Remove(tables);

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        private int GetCurrentUserRestorantId()
        {
            // Kullanıcının kimliğini alın
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının bağlı olduğu restoranın ID'sini veritabanından alın
            var user = _userManager.FindByIdAsync(userId).Result;
            var restorantId = user.RestorantId;

            return restorantId.HasValue ? restorantId.Value : 0; // Varsayılan                   l olarak 0 kullanıldı, siz istediğiniz bir değeri verebilirsiniz.
        }
    }
}
