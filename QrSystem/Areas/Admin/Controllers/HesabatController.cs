using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrSystem.DAL;
using QrSystem.Models;
using QrSystem.Models.Auth;
using System.Security.Claims;

namespace QrSystem.Areas.Admin.Controllers
{
        public class HesabatController : Controller
        {
            private readonly AppDbContext _context;
            private readonly IWebHostEnvironment _env;
            private readonly UserManager<AppUser> _userManager;

            public HesabatController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
            {
                _context = context;
                _env = env;
                _userManager = userManager;
            }

        [Area("Admin")]
        [Authorize(Roles = "Moderator")]
        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            //int restoranId = GetCurrentUserRestorantId();

            //// Tarih aralığı belirtilmediyse tüm Hesabat öğelerini getir
            //IQueryable<Hesabat> query = _context.Hesabats.Include(p => p.Restorant)
            //                                .Where(p => p.RestorantId == restoranId);

            //// Başlangıç ve bitiş tarihleri belirtilmişse, zaman aralığında filtrele
            //if (startDate != null && endDate != null)
            //{
            //    query = query.Where(p => p.DateTime >= startDate && p.DateTime <= endDate);
            //}

            //var hesabatList = query.ToList();

            //// Toplam geliri hesapla
            //double? toplamGelir = hesabatList.Sum(p => p.ToplamGelir);

            //// View'e model olarak gönder
            //ViewBag.ToplamGelir = toplamGelir;
            //return View(hesabatList);

            int restoranId = GetCurrentUserRestorantId();

            IQueryable<Hesabat> query = _context.Hesabats.Include(p => p.Restorant).Include(p=>p.Restorant.Ofisants)
                                        .Where(p => p.RestorantId == restoranId);

            IQueryable<Hesabat> query1 = _context.Hesabats.Include(p => p.Restorant).Include(p => p.Restorant.Ofisants)
                                        .Where(p => p.RestorantId == restoranId);

            // Başlangıç ve bitiş tarihleri belirtilmişse, zaman aralığında filtrele
            if (startDate != null && endDate != null)
            {
                query = query.Where(p => p.DateTime >= startDate && p.DateTime <= endDate)
                             .OrderBy(p => p.DateTime);

                query1 = query1.Where(p => p.DateTime < startDate)
                               .OrderBy(p => p.DateTime);

                var lastItem = query.LastOrDefault();
                var lastItem1 = query1.LastOrDefault();

                if (lastItem != null && lastItem1 != null)
                {
                    lastItem.ToplamGelir -= lastItem1.ToplamGelir;
                    lastItem.OfisantSayi -= lastItem1.OfisantSayi;
                }
            }

            var hesabatList = query.ToList();
            return View(hesabatList);
        }
        private int GetCurrentUserRestorantId()
        {
            // Kullanıcının kimliğini alın
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının bağlı olduğu restoranın ID'sini veritabanından alın
            var user = _userManager.FindByIdAsync(userId).Result;
            var restorantId = user.RestorantId;

            return restorantId.Value;
        }
        //[HttpPost]
        //public IActionResult Filter(DateTime startDate, DateTime endDate)
        //{
        //    int restoranId = GetCurrentUserRestorantId();
        //    var filteredHesabat = _context.Hesabats.Include(p => p.Restorant)
        //                            .Where(p => p.RestorantId == restoranId &&
        //                                        p.DateTime >= startDate &&
        //                                        p.DateTime <= endDate)
        //                            .ToList();
        //    return View("Index", filteredHesabat);
        //}
    }
}
