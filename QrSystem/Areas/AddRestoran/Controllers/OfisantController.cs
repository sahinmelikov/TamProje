using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QrSystem.DAL;
using QrSystem.Models;
using QrSystem.Models.Auth;
using QrSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace QrSystem.Areas.AddRestoran.Controllers
{
    [Area("AddRestoran")]
    public class OfisantController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private const string COOKIES_BASKET = "basketVM";
        public OfisantController(AppDbContext appDbContext, UserManager<AppUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        private void SetBasketItemCountInViewBag()
        {
            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies[COOKIES_BASKET] ?? "[]");
            int itemCount = basketVMs.Sum(b => b.Count);
            ViewBag.BasketItemCount = itemCount;
        }

        public IActionResult Index(int ofisantId)
        {
            var viewModel = new UrunlerViewModel();

            // Ofisantın bağlı olduğu masaların listesini alın
            var ofisantTables = _appDbContext.Tables
                .Where(rt => rt.OfisantId == ofisantId)
                .ToList();

            foreach (var table in ofisantTables)
            {
                // Masaya ait onaylanmış ürünleri alın
                var approvedProducts = _appDbContext.SaxlanilanS
                    .Where(a => !a.IsDeleted && a.QrCodeId == table.QrCodeId)
                    .ToList();

                foreach (var product in approvedProducts)
                {
                    // Ürünlerin QR kodu ve masa numarası bilgilerini ViewModel'e ekleyin
                    if (!viewModel.UrunlerByQrCodeAndTable.ContainsKey(product.QrCodeId))
                    {
                        viewModel.UrunlerByQrCodeAndTable[product.QrCodeId] = new Dictionary<string, List<BasketİtemVM>>();
                    }

                    if (!viewModel.UrunlerByQrCodeAndTable[product.QrCodeId].ContainsKey(product.TableName))
                    {
                        viewModel.UrunlerByQrCodeAndTable[product.QrCodeId][product.TableName] = new List<BasketİtemVM>();
                    }

                    // Zamanı kontrol et ve eğer süre dolmuşsa kırmızı alarma ekleyin
                    var now = DateTime.Now;
                    if (product.DateTime < now)
                    {
                        viewModel.UrunlerByQrCodeAndTable[product.QrCodeId][product.TableName].Add(new BasketİtemVM
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Description = product.Description,
                            Price = product.Price,
                            ProductCount = product.ProductCount,
                            ImagePath = product.ImagePath,
                            TableName = product.TableName,
                            OfisantName = table.Ofisant?.Name, // Ofisantın adını doğrudan tablodan alın ve null kontrolü yapın
                            DateTime = product.DateTime,
                            IsTimeExpired = true // Sürenin dolmuş olduğunu işaretleyin
                        });
                    }
                    else
                    {
                        viewModel.UrunlerByQrCodeAndTable[product.QrCodeId][product.TableName].Add(new BasketİtemVM
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Description = product.Description,
                            Price = product.Price,
                            ProductCount = product.ProductCount,
                            ImagePath = product.ImagePath,
                            TableName = product.TableName,
                            OfisantName = table.Ofisant?.Name, // Ofisantın adını doğrudan tablodan alın ve null kontrolü yapın
                            DateTime = product.DateTime
                        });
                    }
                }
            }
            viewModel.OfisantId = ofisantId;
            return View(viewModel);
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

        [HttpPost]
        public IActionResult ActivateAllProducts(int ofisantID)
        {
            // Tüm ürünlerin ActiveAll özelliğini true yap
            var allProducts = _appDbContext.SaxlanilanS.Where(a => !a.IsDeleted).ToList();
            foreach (var product in allProducts)
            {
                product.ActiveAll = true;
            }
            _appDbContext.SaveChanges();

            return RedirectToAction("Index", new { ofisantID }); // Başarı durumunda 200 OK cevabı döndür
        }
    }
}
