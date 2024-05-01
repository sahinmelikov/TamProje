using QrSystem.Utilities.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QrSystem.DAL;
using QrSystem.Models;
using QrSystem.ViewModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QrSystem.Areas.AddRestoran.Controllers
{
    [Area("AddRestoran")]
    [Authorize(Roles = "Moderator")]
    public class RestorantController : Controller
    {
        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;
        public RestorantController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            return View(_context.Restorant.ToList());
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();
            Restorant restorant = _context.Restorant.Find(id);
            if (restorant is null) return NotFound();
            _context.Restorant.Remove(restorant);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(RestorantCreateVM qrVM)
        {
           
            IFormFile file = qrVM.RestorantImagePath;
            if (!file.ContentType.Contains("image/"))
            {

                ModelState.AddModelError("Image", "Sizin gonderdiyiniz sekil deyil.");
                return View();
            }
            if (!(qrVM.RestorantImagePath.Length / 1024 / 1024 < 2))
            {
                ModelState.AddModelError("Image", "Sizin gonderdiyiniz sekilin olcusu 2mb-dan coxdur.");
                return View();
            }
            string filename = Guid.NewGuid() + file.FileName;
            using (FileStream stream = new FileStream(Path.Combine(_env.WebRootPath, "Restorant", "Image", filename), FileMode.Create))
            {
                file.CopyTo(stream);
            }
            Restorant qrCode = new Restorant
            {
                RestorantName = qrVM.RestorantName,
                Email = qrVM.Email,
                Password = qrVM.Password,
                RestorantImagePath=filename
            };
            _context.Restorant.Add(qrCode);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id == 0) return BadRequest();
            Restorant qrCode = _context.Restorant.Find(id);
            RestorantUpdateVM updateVM = new RestorantUpdateVM()
            {
                RestorantName = qrCode.RestorantName,
                Email= qrCode.Email,
                Password=qrCode.Password
            };
            if (qrCode is null) return NotFound();
            return View(updateVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, RestorantUpdateVM qrCode)
        {
            if (id == null || id == 0 || id != qrCode.Id || qrCode is null) return BadRequest();
            Restorant exist = _context.Restorant.Find(qrCode.Id);
            exist.RestorantName = qrCode.RestorantName;

            if (!qrCode.RestorantImagePath.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Photo", $"{qrCode.RestorantImagePath.FileName} Şekil Tipinde Olmalıdır");
                

                return View(qrCode);
            }

            if (!qrCode.RestorantImagePath.CheckFileSize(1800))
            {
                ModelState.AddModelError("Photo", $"{qrCode.RestorantImagePath.FileName} - 200kb'dan Fazla Olamaz");
                

                return View(qrCode);
            }

            Restorant rest = await _context.Restorant.FindAsync(qrCode.Id);

            if (rest == null)
            {
                return NotFound();
            }

            string rootPath = Path.Combine(_env.WebRootPath, "Restorant", "Image");
            string oldFilePath = Path.Combine(rootPath, rest.RestorantImagePath);

            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            IFormFile file = qrCode.RestorantImagePath;
            if (!file.ContentType.Contains("image/"))
            {

                ModelState.AddModelError("Image", "Sizin gonderdiyiniz sekil deyil.");
                return View();
            }
            if (!(qrCode.RestorantImagePath.Length / 1024 / 1024 < 2))
            {
                ModelState.AddModelError("Image", "Sizin gonderdiyiniz sekilin olcusu 2mb-dan coxdur.");
                return View();
            }
            string filename = Guid.NewGuid() + file.FileName;
            using (FileStream stream = new FileStream(Path.Combine(_env.WebRootPath,"Restorant", "Image",  filename), FileMode.Create))
            {
                file.CopyTo(stream);
            }

            rest.RestorantName = qrCode.RestorantName;
            rest.Email=qrCode.Email;
            rest.Password = qrCode.Password;
            rest.RestorantImagePath = filename;
            

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}