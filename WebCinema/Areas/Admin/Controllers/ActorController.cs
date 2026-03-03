using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace WebActor.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]
    public class ActorController : Controller
    {
        //private ApplicationDbContext _context = new();
        private IRepository<Actor> _actorRepository;
        public ActorController(IRepository<Actor> actorRepository)
        {
            _actorRepository = actorRepository;
        }
        public async Task<IActionResult> Index(string? name, int page = 1)
        {
            //var actors = _context.Actors.AsNoTracking().AsQueryable();
            var actors = await _actorRepository.GetAsync(tracked: false);
            if (name is not null)
            {
                actors = actors.Where(c => c.Name.Contains(name)).ToList();
            }
            if (page < 1) page = 1;

            int currentPage = page;
            double totalPages = Math.Ceiling(actors.Count() / 5.0);
            actors = actors.Skip((page - 1) * 5).Take(5).ToList();

            return View(new ActorsVM
            {
                Actors = actors.AsEnumerable(),
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
        public async Task<IActionResult> Create(Actor actor, IFormFile img)
        {
            if (img == null || img.Length == 0)
            {
                ModelState.AddModelError("Img", "Actor image is required");
            }

            if (!ModelState.IsValid)
            {
                TempData["error-notification"] = "Invalid Data";
                return View(actor);
            }

            if (img is not null && img.Length > 0)
            {
                var newFileName = Guid.NewGuid().ToString().Substring(0, 7) + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(img.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\actor_imgs", newFileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    img.CopyTo(stream);
                }

                // extra login
                actor.Img = newFileName;
            }
            //_context.Actors.Add(actor);
            //_context.SaveChanges();

            await _actorRepository.CreateAsync(actor);
            await _actorRepository.CommitAsync();

            TempData["success-notification"] = "Added Actor Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            //var actor = _context.Actors.Find(id);
            var actor = await _actorRepository.GetOneAsync(e=>e.Id == id);
            if (actor is null) return RedirectToAction(nameof(NotFoundPage));
            return View(actor);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(Actor actor, IFormFile? img)
        {
            if (!ModelState.IsValid)
            {
                TempData["error-notification"] = "Invalid Data";
                return View(actor);
            }

            //Actor? actorInDB = _context.Actors.AsNoTracking().FirstOrDefault(e => e.Id == actor.Id);
            var actorInDB = await _actorRepository.GetOneAsync(e => e.Id == actor.Id,tracked: false);

            if (actorInDB is null) return NotFound();

            if (img is not null && img.Length > 0)
            {
                // Create new img
                var newFileName = Guid.NewGuid().ToString().Substring(0, 7) + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(img.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\actor_imgs", newFileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    img.CopyTo(stream);
                }

                // Delete old img
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\actor_imgs", actorInDB.Img);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update to img in DB
                actor.Img = newFileName;
            }
            else
            {
                actor.Img = actorInDB.Img;
            }
            //_context.Actors.Update(actor);
            //_context.SaveChanges();

            _actorRepository.Update(actor);
            await _actorRepository.CommitAsync();

            TempData["success-notification"] = "Updated Actor Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            //var actor = _context.Actors.Find(id);
            var actor = await _actorRepository.GetOneAsync(e => e.Id == id);


            if (actor is null) return RedirectToAction(nameof(NotFoundPage));
            // Delete img

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\actor_imgs", actor.Img);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            //_context.Actors.Remove(actor);
            //_context.SaveChanges();

            _actorRepository.Delete(actor);
            await _actorRepository.CommitAsync();

            TempData["success-notification"] = "Deleted Actor Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
