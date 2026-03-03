using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace WebCinema.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]
    public class CinemaController : Controller
    {
        //private ApplicationDbContext _context = new();
        private IRepository<Cinema> _cinemaRepository;
        public CinemaController(IRepository<Cinema> cinemaRepository)
        {
            _cinemaRepository = cinemaRepository;
        }
        public async Task<IActionResult> Index(string? name, int page = 1)
        {
            //var cinemas = _context.Cinemas.AsNoTracking().AsQueryable();
            var cinemas = await _cinemaRepository.GetAsync(tracked: false);
            if (name is not null)
            {
                cinemas = cinemas.Where(c => c.Name.Contains(name)).ToList();
            }
            if (page < 1) page = 1;

            int currentPage = page;
            double totalPages = Math.Ceiling(cinemas.Count() / 5.0);
            cinemas = cinemas.Skip((page - 1) * 5).Take(5).ToList();

            return View(new CinemasVM
            {
                Cinemas = cinemas.AsEnumerable(),
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
        public async Task<IActionResult> Create(Cinema cinema, IFormFile img)
        {
            if (img == null || img.Length == 0)
            {
                ModelState.AddModelError("Img", "Cinema image is required");
            }

            if (!ModelState.IsValid)
            {
                TempData["error-notification"] = "Invalid Data";
                return View(cinema);
            }

            if (img is not null && img.Length > 0)
            {
                var newFileName = Guid.NewGuid().ToString().Substring(0, 7) + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(img.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\cinema_imgs", newFileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    img.CopyTo(stream);
                }

                cinema.Img = newFileName;
            }
            //_context.Cinemas.Add(cinema);
            //_context.SaveChanges();

            await _cinemaRepository.CreateAsync(cinema);
            await _cinemaRepository.CommitAsync();

            TempData["success-notification"] = "Added Cinema Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            //var cinema = _context.Cinemas.Find(id);
            var cinema = await _cinemaRepository.GetOneAsync(e=>e.Id == id);
            if (cinema is null) return RedirectToAction(nameof(NotFoundPage));
            return View(cinema);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(Cinema cinema, IFormFile? img)
        {
            if (!ModelState.IsValid)
            {
                TempData["error-notification"] = "Invalid Data";
                return View(cinema);
            }
            //Cinema? cinemaInDB = _context.Cinemas.AsNoTracking().FirstOrDefault(e => e.Id == cinema.Id);
            var cinemaInDB = await _cinemaRepository.GetOneAsync(e => e.Id == cinema.Id,tracked: false);

            if (cinemaInDB is null) return NotFound();

            if (img is not null && img.Length > 0)
            {
                var newFileName = Guid.NewGuid().ToString().Substring(0, 7) + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(img.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\cinema_imgs", newFileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    img.CopyTo(stream);
                }

                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\cinema_imgs", cinemaInDB.Img);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                cinema.Img = newFileName;
            }
            else
            {
                cinema.Img = cinemaInDB.Img;
            }
            //_context.Cinemas.Update(cinema);
            //_context.SaveChanges();

            _cinemaRepository.Update(cinema);
            await _cinemaRepository.CommitAsync();

            TempData["success-notification"] = "Updated Cinema Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(e => e.Id == id);

            if (cinema is null) return RedirectToAction(nameof(NotFoundPage));

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\cinema_imgs", cinema.Img);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            //_context.Cinemas.Remove(cinema);
            //_context.SaveChanges();

            _cinemaRepository.Delete(cinema);
            await _cinemaRepository.CommitAsync();

            TempData["success-notification"] = "Deleted Cinema Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
