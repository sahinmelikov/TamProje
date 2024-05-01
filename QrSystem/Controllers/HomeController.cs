using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrSystem.DAL;
using QrSystem.Models;
using QrSystem.ViewModel;
using System.Diagnostics;

namespace QrSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public HomeController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        [HttpGet]
        public IActionResult Index(int? qrCodeId)
        {
            // Session'dan QR kodunu al
            int? sessionQrCodeId = HttpContext.Session.GetInt32("QrCodeId");

            // URL'den qrCodeId gelmediyse ama session'da varsa, session'daki değeri kullan
            if (!qrCodeId.HasValue && sessionQrCodeId.HasValue)
            {
                qrCodeId = sessionQrCodeId;
            }

            if (qrCodeId.HasValue)
            {
                // Session'a QR kodunu kaydet
                HttpContext.Session.SetInt32("QrCodeId", qrCodeId.Value);
                ViewBag.QrCodeId = qrCodeId.Value;

                // QR koduna göre masa bul
                var masa = _appDbContext.Tables.FirstOrDefault(m => m.QrCodeId == qrCodeId.Value);

                if (masa != null)
                {
                    // QR kodunun restoran ID'sini al
                    var restoranId = _appDbContext.QrCodes
                        .Where(q => q.Id == qrCodeId)
                        .Select(q => q.RestorantId)
                        .FirstOrDefault();

                    if (restoranId.HasValue)
                    {
                        // Restoran ID'sine sahip olan restoranı bul
                        var restoran = _appDbContext.Restorant.FirstOrDefault(r => r.Id == restoranId.Value);

                        if (restoran != null)
                        {
                            // Restoran ID'sine sahip olan ürünleri getir
                            var products = _appDbContext.Products
                                .Where(p => p.RestorantId == restoranId.Value)
                                .ToList();

                            // Restoran ID'sine sahip olan ürünleri getir
                            var products1 = _appDbContext.BigParentCategory
                                .Include(d => d.ParentCategories)
                                .Where(p => p.RestorantId == restoranId.Value)
                                .ToList();
                            // Restoran ID'sine sahip olan ürünleri getir
                            var products2 = _appDbContext.ParentsCategories

                                .Where(p => p.RestorantId == restoranId.Value)
                                .ToList();
                            // HomeVM oluştur ve verileri doldur
                            var homeVM = new HomeVM()
                            {
                                Product = products,
                                RestourantTables = _appDbContext.Tables.Where(d => d.QrCodeId == qrCodeId.Value).ToList(),
                                ParentsCategory = products2,
                                BigParentCategories = products1,

                                Restorants = _appDbContext.Restorant.ToList(),
                                CurrentRestoran = restoran // Restoran bilgisini view modeline ekleyin
                            };
                            return View(homeVM);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Restoran bulunamadı. Lütfen geçerli bir QR kodu girin.");
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Restoran ID'si bulunamadı. Lütfen geçerli bir QR kodu girin.");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Masa bulunamadı. Lütfen geçerli bir QR kodu girin.");
                    return View();
                }
            }
            else
            {
                // qrCodeId değeri yoksa ve daha önce de saklanmamışsa, varsayılan davranış
                return View("ErrorPage"); // Varsayılan davranış olarak bir hata sayfasına yönlendirme
            }
        }




        public IActionResult Sebet()
        {

            HomeVM homeVM = new HomeVM()
            {
                Product = _appDbContext.Products.ToList(),
            };
            return View(homeVM);
        }


    }
}
