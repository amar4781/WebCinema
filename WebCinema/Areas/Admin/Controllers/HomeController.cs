using Microsoft.AspNetCore.Mvc;

namespace WebCinema.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
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
