using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebCinema.ViewModels;

namespace WebCinema.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]
    public class CategoryController : Controller
    {
        //private ApplicationDbContext _context = new();
        private IRepository<Category> _categoryRepository;
        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index(string? name, int page = 1)
        {
            //var categories = _context.Categories.AsNoTracking().AsQueryable();
            var categories = await _categoryRepository.GetAsync(tracked: false);
            if (name is not null)
            {
                categories = categories.Where(c => c.Name.Contains(name)).ToList();
            }
            if (page < 1) page = 1;

            int currentPage = page;
            double totalPages = Math.Ceiling(categories.Count() / 5.0);
            categories = categories.Skip((page - 1) * 5).Take(5).ToList();

            return View(new CategoriesVM
            {
                Categories = categories.AsEnumerable(),
                CurrentPage = currentPage,
                TotalPages = totalPages
            });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                TempData["error-notification"] = "Invalid Data";
                return View(category);
            }
                

            //_context.Categories.Add(category);
            //_context.SaveChanges();

            await _categoryRepository.CreateAsync(category);
            await _categoryRepository.CommitAsync();

            //Response.Cookies.Append("success-notification", "Add Category Successfully");
            TempData["success-notification"] = "Added Category Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var category = await _categoryRepository.GetOneAsync(e=>e.Id == id);
            if (category is null) return RedirectToAction(nameof(NotFoundPage));
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                TempData["error-notification"] = "Invalid Data";
                return View(category);
            }
            //_context.Categories.Update(category);
            //_context.SaveChanges();
            _categoryRepository.Update(category);
             await _categoryRepository.CommitAsync();

            TempData["success-notification"] = "Updated Category Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id);
            if (category is null) return RedirectToAction(nameof(NotFoundPage));
            //_context.Categories.Remove(category);
            //_context.SaveChanges();
            _categoryRepository.Delete(category);
            await _categoryRepository.CommitAsync();

            TempData["success-notification"] = "Deleted Category Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
