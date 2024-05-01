using QrSystem.Utilities.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QrSystem.DAL;
using QrSystem.Models;
using QrSystem.Models.Auth;
using QrSystem.ViewModel;
using System;
using System.Security.Claims;

namespace QrSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Moderator")]
    public class ProductController: Controller
    {
        private readonly UserManager<AppUser> _userManager;
        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;
        public ProductController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
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
                                   .Include(r => r.Products) // Restoranın ürünlerini de getirin
                                   .FirstOrDefault(r => r.Id == restorantId);

            if (restoran == null)
            {
                // Eğer istenen restoran bulunamazsa, hata mesajı gösterin
                ViewBag.ErrorMessage = "No matching restaurant found!";
                return View();
            }

            return View(restoran.Products);
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
            List<ParentCategory> Parent = _context.ParentsCategories.ToList();
            List<SelectListItem> Select = Parent.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            ViewBag.Parents = Select;
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
            var qrVM = new ProductVM { RestorantId = restorantId };
            return View(qrVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductVM productVM)
        {
            // Kullanıcının bağlı olduğu restoranın ID'sini alın
            int restorantId = GetCurrentUserRestorantId();
            IFormFile file = productVM.ImageFile;
            if (!file.ContentType.Contains("image/"))
            {

                ModelState.AddModelError("Image", "Sizin gonderdiyiniz sekil deyil.");
                return View();
            }
            if (!(productVM.ImageFile.Length / 1024 / 1024 < 2))
            {
                ModelState.AddModelError("Image", "Sizin gonderdiyiniz sekilin olcusu 2mb-dan coxdur.");
                return View();
            }
            string filename = Guid.NewGuid() + file.FileName;
            using (FileStream stream = new FileStream(Path.Combine(_env.WebRootPath, "Image", "Product", filename), FileMode.Create))
            {
                file.CopyTo(stream);
            }
            Product product = new Product { Name = productVM.Name,Price= productVM.Price, Description=productVM.Description , ImagePath=filename , Quantity=productVM.Quantity, ParentsCategoryId=productVM.ParentsCategoryId, DateTime = DateTime.Now, RestorantId = restorantId };
            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            Product product = await _context.Products.FindAsync(id);
            List<ParentCategory> Parent = _context.ParentsCategories.ToList();
            List<SelectListItem> Select = Parent.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            ViewBag.Parents = Select;
            ProductUpdateVM updateVM = new ProductUpdateVM()
            {
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ParentsCategoryId = product.ParentsCategoryId,
                Quantity = product.Quantity,
            };

            return View("Update", updateVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(ProductUpdateVM updateVM)
        {
            List<Product> products = _context.Products.Include(m => m.ParentsCategory).ToList();
            List<SelectListItem> Select = products.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            ViewBag.Products = Select;
            if (!updateVM.ImageFile.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Photo", $"{updateVM.ImageFile.FileName} Şekil Tipinde Olmalıdır");
                ViewBag.Products = await _context.Products
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync();

                return View(updateVM);
            }

            if (!updateVM.ImageFile.CheckFileSize(1800))
            {
                ModelState.AddModelError("Photo", $"{updateVM.ImageFile.FileName} - 200kb'dan Fazla Olamaz");
                ViewBag.Products = await _context.Products
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync();

                return View(updateVM);
            }

            Product product = await _context.Products.FindAsync(updateVM.Id);

            if (product == null)
            {
                return NotFound();
            }

            string rootPath = Path.Combine(_env.WebRootPath, "Image", "Product");
            string oldFilePath = Path.Combine(rootPath, product.ImagePath);

            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            IFormFile file = updateVM.ImageFile;
            if (!file.ContentType.Contains("image/"))
            {

                ModelState.AddModelError("Image", "Sizin gonderdiyiniz sekil deyil.");
                return View();
            }
            if (!(updateVM.ImageFile.Length / 1024 / 1024 < 2))
            {
                ModelState.AddModelError("Image", "Sizin gonderdiyiniz sekilin olcusu 2mb-dan coxdur.");
                return View();
            }
            string filename = Guid.NewGuid() + file.FileName;
            using (FileStream stream = new FileStream(Path.Combine(_env.WebRootPath, "Image", "Product", filename), FileMode.Create))
            {
                file.CopyTo(stream);
            }

            product.Name = updateVM.Name;
            product.ImagePath = filename;
            product.Price = updateVM.Price;
            product.Description = updateVM.Description;
            product.Quantity = updateVM.Quantity;
            product.ParentsCategoryId = updateVM.ParentsCategoryId;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int? id)
        {
            if (id is null) return BadRequest();
            Product product= _context.Products.Find(id);
            string rootPath = Path.Combine(_env.WebRootPath, "Image", "Product");
            string oldFilePath = Path.Combine(rootPath, product.ImagePath);

            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
            if (product is null) return NotFound(); 
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));

        }
    }
}
