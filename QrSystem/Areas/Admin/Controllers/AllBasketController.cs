using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QrSystem.DAL;
using QrSystem.Models;
using QrSystem.Models.Auth;
using QrSystem.ViewModel;
using System.Security.Claims;

namespace QrSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Moderator")]
    public class AllBasketController : Controller
    {
        private const string COOKIES_BASKET = "basketVM";
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        public AllBasketController(AppDbContext appDbContext, UserManager<AppUser> userManager)
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






        [HttpPost]
        public IActionResult DeleteProducts(int qrCodeId)
        {
            try
            {
                // Giriş yapılan restoranın ID'sini alın
                int restoranId = GetCurrentUserRestorantId();

                // Veritabanından ilgili restorana ait onaylanmış ürünleri bul
                var productsToDelete = _appDbContext.SaxlanilanS
                    .Where(p => p.QrCodeId == qrCodeId && p.RestorantId == restoranId)
                    .ToList();

                // Bulunan ürünlerin IsDeleted özelliğini true olarak güncelle
                foreach (var product in productsToDelete)
                {
                    product.IsDeleted = true;
                }

                _appDbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Hata durumunda bir hata sayfasına yönlendir
                return RedirectToAction("Error", "Home");
            }
        }
        public IActionResult Index()
        {
            var viewModel = new UrunlerViewModel();

            // Giriş yapılan restoranın ID'sini alın
            int restoranId = GetCurrentUserRestorantId();

            // Tüm onaylanmış ürünleri giriş yapılan restorana ait olanları veritabanından al
            var approvedProducts = _appDbContext.SaxlanilanS?
                .Where(a => !a.IsDeleted && a.RestorantId == restoranId && a.ActiveAll == true) // Sadece ActiveAll true olanları seç
                .ToList();
            var Ofisant = _appDbContext.Ofisant.ToList();

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

                // Her bir sipariş için ayrı bir BasketİtemVM oluşturun
                var basketItem = new BasketİtemVM
                {
                    Id = product.Id,
                    ProductId = product.ProductId,
                    QrCodeId = product.QrCodeId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ProductCount = product.ProductCount,
                    ImagePath = product.ImagePath,
                    TableName = product.TableName,
                    OfisantName = product.OfisantName
                };

                // ViewModel'e ekle
                viewModel.UrunlerByQrCodeAndTable[product.QrCodeId][product.TableName].Add(basketItem);
            }

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
        public IActionResult TimeBashlat(int selectedMinute, int productId, int qrCodeId, int siparisId)
        {
            var siparis = _appDbContext.SaxlanilanS.FirstOrDefault(s => s.Id == siparisId && s.ProductId == productId && s.QrCodeId == qrCodeId && !s.IsDeleted);

            if (siparis != null)
            {
                // Seçilen dakikayı DateTime nesnesine ekleyin
                siparis.DateTime = DateTime.Now.AddMinutes(selectedMinute);
                _appDbContext.SaveChanges();

                ViewBag.ProductId = productId; // ViewBag'e productId değerini ekleyin
                return RedirectToAction("Index"); // Başka bir sayfaya yönlendirilebilir
            }

            return NotFound(); // Sipariş bulunamadı durumu
        }


        //private Dictionary<int, Dictionary<string, List<BasketİtemVM>>> GetApprovedProducts()
        //{
        //    var approvedProductsByQrCodeAndTable = new Dictionary<int, Dictionary<string, List<BasketİtemVM>>>();

        //    // Burada, onaylanmış ürünleri sakladığınız mekanizmayı kullanarak veriyi çekin
        //    // Örneğin, veritabanından onaylanmış ürünleri çekin
        //    // Örnek bir sorgu:
        //    var approvedProducts = _appDbContext.SaxlanilanS.ToList();

        //    // Her bir onaylanmış ürünü uygun şekilde yerleştirin
        //    // Örneğin:
        //    foreach (var product in approvedProducts)
        //    {
        //        var qrCodeId = product.QrCodeId;
        //        var tableName = product.TableName;
        //        var basketItem = new BasketİtemVM
        //        {
        //            Name = product.Name,
        //            ProductId = product.Id,
        //            Description = product.Description,
        //            Price = product.Price,
        //            ProductCount = product.ProductCount,
        //            ImagePath = product.ImagePath,
        //            TableName = tableName

        //        };

        //        if (!approvedProductsByQrCodeAndTable.ContainsKey(qrCodeId))
        //        {
        //            approvedProductsByQrCodeAndTable[qrCodeId] = new Dictionary<string, List<BasketİtemVM>>();
        //        }

        //        if (!approvedProductsByQrCodeAndTable[qrCodeId].ContainsKey(tableName))
        //        {
        //            approvedProductsByQrCodeAndTable[qrCodeId][tableName] = new List<BasketİtemVM>();
        //        }

        //        approvedProductsByQrCodeAndTable[qrCodeId][tableName].Add(basketItem);
        //    }

        //    return approvedProductsByQrCodeAndTable;
        //}


        //private List<SaxlanilanSifarish> GetProductsInCart(int qrCodeId)
        //{
        //    List<SaxlanilanSifarish> basketItemVMs = new List<SaxlanilanSifarish>();

        //    var basketCookieName = COOKIES_BASKET + "_" + qrCodeId; // Her QR kodu için farklı bir cookie adı oluştur
        //    List<SaxlanilanSifarish> basketVMs = JsonConvert.DeserializeObject<List<SaxlanilanSifarish>>(Request.Cookies[basketCookieName] ?? "[]");

        //    foreach (var item in basketVMs)
        //    {
        //        Product product = _appDbContext.Products
        //            .Include(p => p.Tables)
        //            .FirstOrDefault(p => p.Id == item.ProductId);

        //        if (product != null)
        //        {
        //            var tableName = item.TableName.ToString();
        //            var existingItem = basketItemVMs.FirstOrDefault(b => b.ProductId == product.Id && b.TableName == tableName);
        //            if (existingItem != null)
        //            {
        //                existingItem.ProductCount += item.ProductCount; // Ürün zaten varsa miktarını artır
        //            }
        //            else
        //            {
        //                basketItemVMs.Add(new SaxlanilanSifarish
        //                {
        //                    DateTime= DateTime.Now,
        //                    Name = product.Name,
        //                    ProductId = product.Id,
        //                    Description = product.Description,
        //                    Price = product.Price,
        //                    ProductCount = item.ProductCount,
        //                    ImagePath = product.ImagePath,
        //                    TableName = tableName
        //                });
        //            }
        //        }
        //    }

        //    _appDbContext.SaveChanges();
        //    return basketItemVMs;
        //}
    }
}
