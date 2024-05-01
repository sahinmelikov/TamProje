using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrSystem.DAL;
using QrSystem.Models;
using QrSystem.ViewModel;

namespace QrSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Moderator")]
    public class ParentController: Controller
    {
        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;
        public ParentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            return View(_context.ParentsCategories.ToList());
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();
            ParentCategory parents = _context.ParentsCategories.Find(id);
            if (parents is null) return NotFound();
            _context.ParentsCategories.Remove(parents);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ParentCategoryVM parentCategoryVM)
        {
            ParentCategory parents= new ParentCategory { Name= parentCategoryVM.Name, Isactive=parentCategoryVM.Isactive , DateTime = DateTime.Now };
            _context.ParentsCategories.Add(parents);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id == 0) return BadRequest();
            ParentCategory parent = _context.ParentsCategories.Find(id);
            if (parent is null) return NotFound();
            return View(parent);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, ParentCategory Parent)
        {
            if (id == null || id == 0 || id != Parent.Id || Parent is null) return BadRequest();
            ParentCategory exist = _context.ParentsCategories.Find(Parent.Id);
            exist.Name = Parent.Name;
            exist.Isactive = Parent.Isactive;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
