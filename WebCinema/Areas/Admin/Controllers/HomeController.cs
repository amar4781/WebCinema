using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebCinema.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles =$"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]
    public class HomeController : Controller
    {
        private ApplicationDbContext _context = new();
        public IActionResult Index()
        {
            return View(new DashboardCountVM
            {
                MoviesCount = _context.Movies.Count(),
                CinemasCount = _context.Cinemas.Count(),
                ActorsCount = _context.Actors.Count(),
                CategoriesCount = _context.Categories.Count()
            });
        }
    }
}
