using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    public class OfisantController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;
        public OfisantController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Kullanıcının bağlı olduğu restoranın ID'sini alın
            int restorantId = GetCurrentUserRestorantId();

            // Kullanıcının bağlı olduğu restorana ait ofisantları getirin
            var ofisantlar = _context.Ofisant.Include(o => o.Restorant).Where(o => o.RestorantId == restorantId).ToList();

            return View(ofisantlar);
        }

        public IActionResult Create()
        {
            int restorantId = GetCurrentUserRestorantId();
            if (restorantId == 0)
            {
                // Hata mesajı göstermek için ModelState kullanabilirsiniz
                ModelState.AddModelError(string.Empty, "Restoran bulunamadı.");
                return View();
            }

            // Restoran ID'si bulunduysa, yeni bir QR kodu oluşturmak için kullanabiliriz
            var qrVM = new OfisantVM { RestorantId = restorantId };
            return View(qrVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OfisantVM ofisantvm)
        {
                int restorantId = GetCurrentUserRestorantId();


                // Kullanıcının bağlı olduğu restoranın ID'sini alın

                ofisantvm.RestorantId = restorantId;
                

                // Ofisant ekleme işlemi yapıldığında ofisant sayısını artır
                var hesabat = await _context.Hesabats.FindAsync(restorantId);
                if (hesabat != null)
                {
                    hesabat.OfisantSayi++;
                }
                Ofisant ofisant = new Ofisant { DateTime = DateTime.Now , Name=ofisantvm.Name, RestorantId=restorantId};
                _context.Ofisant.Add(ofisant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id == 0) return BadRequest();
            RestourantTables table = _context.Tables.Find(id);
            if (table is null) return NotFound();
            return View(table);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Ofisant ofisant)
        {
            if (id != ofisant.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ofisant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfisantExists(ofisant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ofisant);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var ofisant = await _context.Ofisant.FindAsync(id);
            if (ofisant == null)
            {
                return NotFound();
            }

            return View(ofisant);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ofisant = await _context.Ofisant.FindAsync(id);
            _context.Ofisant.Remove(ofisant);

            // Ofisant silme işlemi yapıldığında ofisant sayısını azalt
            var restorant = await _context.Hesabats.FindAsync(ofisant.RestorantId);
            if (restorant != null)
            {
                restorant.OfisantSayi--;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OfisantExists(int id)
        {
            return _context.Ofisant.Any(e => e.Id == id);
        }

        private int GetCurrentUserRestorantId()
        {
            // Kullanıcının kimliğini alın
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının bağlı olduğu restoranın ID'sini veritabanından alın
            var user = _userManager.FindByIdAsync(userId).Result;
            var restorantId = user.RestorantId;

            return restorantId.HasValue ? restorantId.Value : 0;
        }
    }
}
