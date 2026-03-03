using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace WebCinema.Areas.Customer.Controllers
{
    [Area(SD.CUSTOMER_AREA)]
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index(int? categoryId)
        {
            var movies = _context.Movies
                .Include(e => e.Category)
                .AsQueryable();

            if (categoryId is not null)
                movies = movies.Where(e => e.CategoryId == categoryId);

            movies = movies.Skip(0).Take(4);

            var categories = _context.Categories.AsQueryable();

            return View(new MoviesWithCategoriesVM
            {
                Movies = movies.ToList(),
                Categories = categories.ToList()
            });
        }

        public IActionResult Details(int id)
        {
            var movie = _context.Movies.SingleOrDefault(e => e.Id == id);
    
                if (movie is null) return NotFound();

            var categories = _context.Categories.AsQueryable();
            var movieSubImgs = _context.MovieSubImgs.Where(e => e.MovieId == id);
            var sameCategories = _context.Movies.Where(e => e.CategoryId == movie.CategoryId && e.Id != movie.Id).Skip(0).Take(4);
            var relatedProducts = _context.Movies.Where(e => e.Name.Contains(movie.Name) && e.Id != movie.Id).Skip(0).Take(4);
            return View(new MovieWithRelatedVM
            {
                Movie = movie,
                SameCategories = sameCategories.ToList(),
                Categories = categories.ToList(),
                MovieSubImgs = movieSubImgs.ToList(),
                RelatedMovies = relatedProducts.ToList(),
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
