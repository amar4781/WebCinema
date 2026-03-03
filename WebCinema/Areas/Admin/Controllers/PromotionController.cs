using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebCinema.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]
    public class PromotionController : Controller
    {
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Movie> _movieRepository;

        public PromotionController(IRepository<Promotion> promotionRepository, IRepository<Movie> movieRepository)
        {
            _promotionRepository = promotionRepository;
            _movieRepository = movieRepository;
        }
        public async Task<IActionResult> Index(string? code, int page = 1)
        {
            var promotions = await _promotionRepository.GetAsync(includes: [p => p.Movie], tracked: false);
            if (code is not null)
            {
                promotions = promotions.Where(c => c.Code.Contains(code)).ToList();
            }
            if (page < 1) page = 1;

            int currentPage = page;
            double totalPages = Math.Ceiling(promotions.Count() / 5.0);
            promotions = promotions.Skip((page - 1) * 5).Take(5).ToList();

            return View(new PromotionsVM
            {
                Promotions = promotions.AsEnumerable(),
                CurrentPage = currentPage,
                TotalPages = totalPages
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var movies = await _movieRepository.GetAsync(tracked: false);

            return View(new PromotionCreateVM
            {
                Promotion = new Promotion(),
                Movies = movies.AsEnumerable(),
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionCreateVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["error-notification"] = "Invalid Data";
                vm.Movies = await _movieRepository.GetAsync(tracked: false);
                return View(vm);
            }

            await _promotionRepository.CreateAsync(vm.Promotion);
            await _promotionRepository.CommitAsync();

            TempData["success-notification"] = "Promotion created successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var promotion = await _promotionRepository.GetOneAsync(e => e.Id == id, includes: [p => p.Movie]);

            if (promotion is null) return RedirectToAction(nameof(NotFoundPage));

            var movies = await _movieRepository.GetAsync(tracked: false);

            return View(new PromotionUpdateResponseVM
            {
                Promotion = promotion,
                Movies = movies.AsEnumerable(),
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(PromotionUpdateResponseVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["error-notification"] = "Invalid Data";
                return View(vm);
            }

            _promotionRepository.Update(vm.Promotion);
            await _promotionRepository.CommitAsync();

            TempData["success-notification"] = "Promotion updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var promotion = await _promotionRepository.GetOneAsync(e => e.Id == id);

            if (promotion is null) return RedirectToAction(nameof(NotFoundPage));

            _promotionRepository.Delete(promotion);
            await _promotionRepository.CommitAsync();

            TempData["success-notification"] = "Promotion deleted successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
