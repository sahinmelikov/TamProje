using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QrSystem.DAL;
using QrSystem.Models;
using QrSystem.ViewModel;

namespace QrSystem.Controllers
{
    public class BasketController : Controller
    {

        private const string COOKIES_BASKET = "basketVM";
        private readonly AppDbContext _appDbContext;

        public BasketController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        private void SetBasketItemCountInViewBag()
        {
            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies[COOKIES_BASKET] ?? "[]");
            int itemCount = basketVMs.Sum(b => b.Count);
            ViewBag.BasketItemCount = itemCount;
        }
        [HttpGet]
        public IActionResult Index(int? qrCodeId)
        {
            if (!qrCodeId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "QR kodu belirtilmedi.");
                return View(new List<BasketİtemVM>());
            }

            var masa = _appDbContext.Tables
                                     .Include(t => t.Products)
                                     .FirstOrDefault(m => m.QrCodeId == qrCodeId);
            if (masa == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz QR kodu. Lütfen doğru bir QR kodu girin.");
                return View(new List<BasketİtemVM>());
            }

            ViewBag.QrCodeId = qrCodeId;
            // QR koduna ait restoran bilgisini bulun
            var restoranId = _appDbContext.QrCodes
                .Where(q => q.Id == qrCodeId)
                .Select(q => q.RestorantId)
                .FirstOrDefault();

            // Restoran bilgisini kullanarak restoran adını bulun
            var restoranName = _appDbContext.Restorant
                .Where(r => r.Id == restoranId)
                .Select(r => r.RestorantName)
                .FirstOrDefault();
            ViewBag.RestoranName = restoranName;
            var basketCookieName = COOKIES_BASKET + "_" + qrCodeId;
            List<BasketVM> basketVMList = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies[basketCookieName] ?? "[]");

            List<BasketİtemVM> basketItemVMs = new List<BasketİtemVM>();

            foreach (var basketItem in basketVMList)
            {
                var product = _appDbContext.Products.Include(o => o.Restorant.Ofisants).FirstOrDefault(p => p.Id == basketItem.ProductId);
                if (product != null)
                {
                    basketItemVMs.Add(new BasketİtemVM
                    {
                        Name = product.Name,
                        Id = product.Id,
                        Price = product.Price,
                        ImagePath = product.ImagePath,
                        QrCodeId = qrCodeId.Value,
                        OfisantName = product.Restorant.Ofisants.FirstOrDefault().Name,
                        ProductId = basketItem.ProductId,
                        ProductCount = basketItem.Count // Ürün sayısını burada ekleyin
                    });
                }
            }

            return View(basketItemVMs);
        }


        [HttpPost]
        public IActionResult AddBasket(int qrCodeId, int productId)
        {
            var masa = _appDbContext.Tables.FirstOrDefault(m => m.QrCodeId == qrCodeId);
            if (masa == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz QR kodu. Lütfen doğru bir QR kodu girin.");
                return RedirectToAction("Index");
            }

            var product = _appDbContext.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                ModelState.AddModelError(string.Empty, "Ürün bulunamadı. Lütfen geçerli bir ürün seçin.");
                return RedirectToAction("Index", new { qrCodeId });
            }

            var basketCookieName = COOKIES_BASKET + "_" + qrCodeId; // Her QR kodu için farklı bir cookie adı oluştur
            List<BasketVM> basketVMList = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies[basketCookieName] ?? "[]");

            var basketItem = basketVMList.FirstOrDefault(b => b.ProductId == productId);
            if (basketItem != null)
            {
                basketItem.Count++;
            }
            else
            {
                basketVMList.Add(new BasketVM { ProductId = productId, Count = 1 });
            }

            var cookieOptions = new CookieOptions { Expires = DateTime.Now.AddDays(1) };
            Response.Cookies.Append(basketCookieName, JsonConvert.SerializeObject(basketVMList), cookieOptions);

            return RedirectToAction("Index", "Home", new { qrCodeId });
        }



        public IActionResult RemoveFromBasket(int id)
        {
            List<BasketVM> basketVMList = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies[COOKIES_BASKET] ?? "[]");

            BasketVM cookiesBasket = basketVMList.FirstOrDefault(s => s.ProductId == id);
            if (cookiesBasket != null)
            {
                if (cookiesBasket.Count > 1)
                {
                    cookiesBasket.Count--;
                }
                else
                {
                    basketVMList.Remove(cookiesBasket);
                }
            }

            Response.Cookies.Append(COOKIES_BASKET, JsonConvert.SerializeObject(basketVMList.OrderBy(s => s.ProductId)));

            SetBasketItemCountInViewBag();

            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult RemoveItemFromBasket(int qrCodeId, int id)
        {
            // Sepetten ürünü kaldır
            string basketCookieName = COOKIES_BASKET + "_" + qrCodeId;
            List<BasketVM> basketVMList = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies[basketCookieName] ?? "[]");

            BasketVM productToRemove = basketVMList.FirstOrDefault(s => s.ProductId == id);
            if (productToRemove != null)
            {
                basketVMList.Remove(productToRemove);
                Response.Cookies.Append(basketCookieName, JsonConvert.SerializeObject(basketVMList.OrderBy(s => s.ProductId)));
            }

            // Veritabanındaki siparişi güncelle
            var ordersToUpdate = _appDbContext.SaxlanilanS
                                .Where(o => o.QrCodeId == qrCodeId && o.ProductId == id && !o.IsDeleted).ToList();

            if (ordersToUpdate.Any())
            {
                foreach (var order in ordersToUpdate)
                {
                    // Siparişin oluşturulma zamanını kontrol et
                    var now = DateTime.Now;
                    var difference = (now - order.DateTime).TotalMinutes;

                    // Belirtilen süre (örneğin 4 dakika) içinde mi kontrol et
                    if (difference <= 1)
                    {
                        // Belirtilen süre içindeyse, siparişi sil
                        order.ProductCount = 0; // Ürün miktarını 0 yap
                        order.IsDeleted = true;
                        _appDbContext.SaveChanges();
                    }
                    else
                    {
                        // Belirtilen sürenin dışında ise, isDeleted'i true yap
                        order.IsDeleted = false;
                        _appDbContext.SaveChanges();
                    }
                }
            }

            SetBasketItemCountInViewBag();
            return RedirectToAction("Index", new { qrCodeId });
        }

        private DateTime? GetConfirmationTime(int qrCodeId)
        {
            // Siparişin onaylanma zamanını veritabanından al veya başka bir yerden alabilirsiniz.
            // Örnek olarak, burada veritabanından alınması durumunda bir örnek veriyorum:

            var order = _appDbContext.SaxlanilanS
                                    .FirstOrDefault(o => o.QrCodeId == qrCodeId && !o.IsDeleted);

            if (order != null && order.DateTime != null)
            {
                return order.DateTime;
            }

            return null;
        }

        [HttpPost]
        public IActionResult UpdateBasketItem(int qrCodeId, int productId, string action)
        {
            var basketCookieName = COOKIES_BASKET + "_" + qrCodeId;
            List<BasketVM> basketVMList = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies[basketCookieName] ?? "[]");

            BasketVM basketItem = basketVMList.FirstOrDefault(s => s.ProductId == productId);
            if (basketItem != null)
            {
                if (action == "increment")
                {
                    basketItem.Count++;

                    // Ürün miktarını azalt
                    Product product = _appDbContext.Products.FirstOrDefault(p => p.Id == productId);
                    if (product != null)
                    {
                        product.Quantity--;
                        _appDbContext.SaveChanges();
                    }
                }
                else if (action == "decrement")
                {
                    if (basketItem.Count > 1)
                    {
                        basketItem.Count--;

                        // Ürün miktarını azalt
                        Product product = _appDbContext.Products.FirstOrDefault(p => p.Id == productId);
                        if (product != null)
                        {
                            product.Quantity++; // Ürün miktarını artır
                            _appDbContext.SaveChanges();
                        }
                    }
                    else
                    {
                        basketItem.Count = 1; // Ürün miktarını sadece 1 tane yap
                    }
                }


            }

            Response.Cookies.Append(basketCookieName, JsonConvert.SerializeObject(basketVMList.OrderBy(s => s.ProductId)));

            SetBasketItemCountInViewBag();

            return RedirectToAction("Index", new { qrCodeId });
        }
        [HttpPost]
        public IActionResult RemoveAllItemsFromBasket(int qrCodeId)
        {
            string basketCookieName = COOKIES_BASKET + "_" + qrCodeId;

            // Sepet öğelerini temizle
            Response.Cookies.Delete(basketCookieName);

            // Veritabanındaki ilgili tüm ürün kayıtlarını güncelle
            var productsInDb = _appDbContext.SaxlanilanS.Where(p => p.QrCodeId == qrCodeId).ToList();
            foreach (var product in productsInDb)
            {
                product.ProductCount = 0;
                product.IsDeleted = true;
            }
            _appDbContext.SaveChanges();

            return RedirectToAction("Index", new { qrCodeId });
        }

        [HttpPost]
        public IActionResult Checkout(int qrCodeId)
        {
            // Belirli bir QR koduna sahip sepet ürünlerini al
            List<BasketİtemVM> productsInCart = GetProductsInCart(qrCodeId);

            // Sepetteki ürünleri onayla ve veritabanına kaydet
            ConfirmAndSaveProducts(qrCodeId, productsInCart);

            // urun.cshtml sayfasına yönlendir
            return RedirectToAction("Index", new { qrCodeId });
        }
        private void ConfirmAndSaveProducts(int qrCodeId, List<BasketİtemVM> approvedProducts)
        {
            foreach (var product in approvedProducts)
            {
                // Veritabanında ürün var mı ve silinmemiş mi diye kontrol et
                var existingOrder = _appDbContext.SaxlanilanS
                    .FirstOrDefault(o => o.ProductId == product.ProductId && o.QrCodeId == qrCodeId && !o.IsDeleted);

                if (existingOrder != null)
                {
                    // Eğer ürünün teslim zamanı belirlenmişse, yeni bir sipariş gibi kaydet
                    if (existingOrder.DateTime != null)
                    {
                        // Yeni bir sipariş oluştur
                        var newOrder = new SaxlanilanSifarish
                        {
                            Comment = product.Comment,
                            QrCodeId = qrCodeId,
                            Name = product.Name,
                            ProductId = product.ProductId,
                            Description = product.Description,
                            Price = product.Price,
                            ProductCount = product.ProductCount, // Onaylanan miktar
                            ImagePath = product.ImagePath,
                            TableName = product.TableName,
                            RestorantId = product.ResTorantId,
                            IsDeleted = false, // Yeni kayıtlar için IsDeleted varsayılan olarak false olmalı
                            DateTime = DateTime.Now,
                        };
                        _appDbContext.SaxlanilanS.Add(newOrder);
                    }
                    else
                    {
                        // Ürün zaten varsa ve silinmemişse, miktarı gelen miktar ile güncelle
                        existingOrder.ProductCount = product.ProductCount; // Doğrudan onaylanan miktarı ata
                    }
                }
                else
                {
                    // Eğer ürün veritabanında yoksa veya silinmişse, yeni bir kayıt ekle
                    var newOrder = new SaxlanilanSifarish
                    {
                        Comment = product.Comment,
                        QrCodeId = qrCodeId,
                        Name = product.Name,
                        ProductId = product.ProductId,
                        Description = product.Description,
                        Price = product.Price,
                        ProductCount = product.ProductCount, // Onaylanan miktar
                        ImagePath = product.ImagePath,
                        TableName = product.TableName,
                        RestorantId = product.ResTorantId,
                        IsDeleted = false, // Yeni kayıtlar için IsDeleted varsayılan olarak false olmalı
                        DateTime = DateTime.Now
                    };
                    _appDbContext.SaxlanilanS.Add(newOrder);
                }
            }

            // Değişiklikleri veritabanına kaydet
            _appDbContext.SaveChanges();
        }


        //public IActionResult Urun()
        //{
        //    var viewModel = new UrunlerViewModel();

        //    foreach (var key in HttpContext.Session.Keys)
        //    {
        //        if (key.StartsWith("ApprovedProducts-"))
        //        {
        //            var qrCodeId = int.Parse(key.Split('-')[1]);
        //            var productsJson = HttpContext.Session.GetString(key);
        //            var approvedProducts = JsonConvert.DeserializeObject<List<BasketİtemVM>>(productsJson);

        //            // QR kodu ve ona ait ürünleri ViewModel'e ekleyin
        //            if (!viewModel.UrunlerByQrCodeAndTable.ContainsKey(qrCodeId))
        //            {
        //                viewModel.UrunlerByQrCodeAndTable[qrCodeId] = new Dictionary<string, List<BasketİtemVM>>();
        //            }

        //            foreach (var product in approvedProducts)
        //            {
        //                if (!viewModel.UrunlerByQrCodeAndTable[qrCodeId].ContainsKey(product.TableName))
        //                {
        //                    viewModel.UrunlerByQrCodeAndTable[qrCodeId][product.TableName] = new List<BasketİtemVM>();
        //                }
        //                viewModel.UrunlerByQrCodeAndTable[qrCodeId][product.TableName].Add(product);
        //            }
        //        }
        //    }

        //    return View(viewModel); // Burada viewModel nesnesini görünüme geçirin
        //}

        private void SaveApprovedProductsToSession(int qrCodeId, List<BasketİtemVM> approvedProducts)
        {
            var sessionKey = $"ApprovedProducts-{qrCodeId}";
            HttpContext.Session.SetString(sessionKey, JsonConvert.SerializeObject(approvedProducts));
        }

        private List<BasketİtemVM> GetProductsInCart(int qrCodeId)
        {
            List<BasketİtemVM> basketItemVMs = new List<BasketİtemVM>();

            var basketCookieName = COOKIES_BASKET + "_" + qrCodeId; // Her QR kodu için farklı bir cookie adı oluştur
            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies[basketCookieName] ?? "[]");

            foreach (var item in basketVMs)
            {
                Product product = _appDbContext.Products
                    .Include(p => p.Tables)
                    .FirstOrDefault(p => p.Id == item.ProductId);

                if (product != null)
                {
                    var tableName = item.TableNumber.ToString();
                    var existingItem = basketItemVMs.FirstOrDefault(b => b.ProductId == product.Id);
                    if (existingItem != null)
                    {
                        existingItem.ProductCount += item.Count; // Ürün zaten varsa miktarını artır
                    }
                    else
                    {
                        basketItemVMs.Add(new BasketİtemVM
                        {
                            Name = product.Name,
                            ProductId = product.Id,
                            Description = product.Description,
                            Price = product.Price,
                            ProductCount = item.Count,
                            ImagePath = product.ImagePath,
                            ResTorantId = product.RestorantId,
                            TableName = tableName,

                        });
                    }
                }
            }
            _appDbContext.SaveChanges();
            return basketItemVMs;
        }

    }

}

