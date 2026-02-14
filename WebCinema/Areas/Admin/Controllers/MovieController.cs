using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace WebCinema.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class MovieController : Controller
    {
        //private ApplicationDbContext _context = new();
        private IRepository<Category> _categoryRepository;
        private IRepository<Cinema> _cinemaRepository;
        private IRepository<Actor> _actorRepository;
        private IRepository<Movie> _movieRepository;
        private IMovieSubImgRepository _movieSubImgRepository;
        private IMovieActorRepository _movieActorRepository;

        public MovieController(IRepository<Category> categoryRepository, IRepository<Cinema> cinemaRepository, IRepository<Actor> actorRepository, IRepository<Movie> movieRepository, IMovieSubImgRepository movieSubImgRepository, IMovieActorRepository movieActorRepository)
        {
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
            _actorRepository = actorRepository;
            _movieRepository = movieRepository;
            _movieSubImgRepository = movieSubImgRepository;
            _movieActorRepository = movieActorRepository;
        }

        public async Task<IActionResult> Index(string? name, int page = 1)
        {
            //var movies = _context.Movies.AsNoTracking().AsQueryable();
            var movies = await _movieRepository.GetAsync(includes: [e=> e.Category, e => e.Cinema] ,tracked: false);
            //movies = movies.Include(e => e.Category).Include(e => e.Cinema);
            if (name is not null)
            {
                movies = movies.Where(c => c.Name.Contains(name)).ToList();
            }
            if (page < 1) page = 1;

            int currentPage = page;
            double totalPages = Math.Ceiling(movies.Count() / 5.0);
            movies = movies.Skip((page - 1) * 5).Take(5).ToList();

            return View(new MoviesVM
            {
                Movies = movies.AsEnumerable(),
                CurrentPage = currentPage,
                TotalPages = totalPages
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //var categories = _context.Categories.AsNoTracking().Where(c => c.Status).AsQueryable();
            //var cinemas = _context.Cinemas.AsNoTracking().Where(c => c.Status).AsQueryable();
            //var actors = _context.Actors.AsNoTracking().AsQueryable();

            var categories = await _categoryRepository.GetAsync(tracked: false);
            var cinemas = await _cinemaRepository.GetAsync(tracked: false);
            var actors = await _actorRepository.GetAsync(tracked: false);

            return View(new MovieCreateVM
            {
                //Movie = new Movie(),
                Categories = categories.AsEnumerable(),
                Cinemas = cinemas.AsEnumerable(),
                Actors = actors.AsEnumerable(),
                //SelectedActorIds = new List<int>(),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Movie movie, IFormFile MainImg, List<IFormFile>? SubImgs, List<int>? ActorIds)
        {
            if (MainImg is not null && MainImg.Length > 0)
            {
                var newFileName = Guid.NewGuid().ToString().Substring(0, 7) + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(MainImg.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\movie_imgs", newFileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    MainImg.CopyTo(stream);
                }

                movie.MainImg = newFileName;
            }

            //_context.Movies.Add(movie);
            //_context.SaveChanges();

            await _movieRepository.CreateAsync(movie);
            await _movieRepository.CommitAsync();

            if (ActorIds is not null && ActorIds.Any())
            {
                foreach (var actorId in ActorIds)
                {
                    //_context.MovieActors.Add(new MovieActor
                    //{
                    //    MovieId = movie.Id,
                    //    ActorId = actorId
                    //});
                    await _movieActorRepository.CreateAsync(new MovieActor
                    {
                        MovieId = movie.Id,
                        ActorId = actorId
                    });
                }
                //_context.SaveChanges();
                await _movieActorRepository.CommitAsync();
            }

            if (SubImgs != null && SubImgs.Any())
            {
                foreach (var item in SubImgs)
                {
                    var newFileName = Guid.NewGuid().ToString().Substring(0, 7) + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(item.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\movie_imgs\\sub_images", newFileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    //_context.MovieSubImgs.Add(new()
                    //{
                    //    MovieId = movie.Id,
                    //    SubImg = newFileName,
                    //});

                    await _movieSubImgRepository.CreateAsync(new()
                    {
                        MovieId = movie.Id,
                        SubImg = newFileName,
                    });
                }
                //_context.SaveChanges();
                await _movieSubImgRepository.CommitAsync();
            }

            TempData["success-notification"] = "Added Movie Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            //var movie = _context.Movies.Find(id);
            var movie = await _movieRepository.GetOneAsync(e => e.Id == id, tracked: true);
            if (movie is null) return RedirectToAction(nameof(NotFoundPage));

            //var movieSubImgs = _context.MovieSubImgs.Where(e => e.MovieId == id);
            var movieSubImgs = await _movieSubImgRepository.GetAsync(e => e.MovieId == id);
            //var selectedActorIds = _context.MovieActors.Where(e => e.MovieId == id).Select(e => e.ActorId).ToList();
            var movieActors = await _movieActorRepository.GetAsync(e => e.MovieId == id);
            var selectedActorIds = movieActors.Select(e => e.ActorId).ToList();
            //var categories = _context.Categories.AsQueryable();
            //var cinemas = _context.Cinemas.AsQueryable();
            //var actors = _context.Actors.AsQueryable();
            var categories = await _categoryRepository.GetAsync(tracked: false);
            var cinemas = await _cinemaRepository.GetAsync(tracked: false);
            var actors = await _actorRepository.GetAsync(tracked: false);

            return View(new MovieUpdateResponseVM()
            {
                Movie = movie,
                SelectedActorIds = selectedActorIds,
                MovieSubImgs = movieSubImgs.AsEnumerable(),
                Categories = categories.AsEnumerable(),
                Cinemas = cinemas.AsEnumerable(),
                Actors = actors.AsEnumerable(),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Movie movie, IFormFile? MainImg, List<IFormFile>? SubImgs, List<int> ActorIds)
        {
            //var movieInDb = _context.Movies.AsNoTracking().FirstOrDefault(e => e.Id == movie.Id);
            var movieInDb = await _movieRepository.GetOneAsync(e => e.Id == movie.Id, tracked: false);

            if (movieInDb is null) return NotFound();

            if (MainImg is not null && MainImg.Length > 0)
            {
                var newFileName = Guid.NewGuid().ToString().Substring(0, 7) + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(MainImg.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\movie_imgs", newFileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    MainImg.CopyTo(stream);
                }

                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\movie_imgs", movieInDb.MainImg);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                movie.MainImg = newFileName;
            }
            else
            {
                movie.MainImg = movieInDb.MainImg;
            }

            //_context.Movies.Update(movie);
            //_context.SaveChanges();

            _movieRepository.Update(movie);
            await _movieRepository.CommitAsync();

            //var oldMovieActors = _context.MovieActors.Where(ma => ma.MovieId == movie.Id);
            //_context.MovieActors.RemoveRange(oldMovieActors);

            var oldMovieActors = await _movieActorRepository.GetAsync(ma => ma.MovieId == movie.Id);
            _movieActorRepository.DeleteRange(oldMovieActors);

            if (ActorIds is not null && ActorIds.Any())
            {
                foreach (var actorId in ActorIds)
                {
                    //_context.MovieActors.Add(new MovieActor
                    //{
                    //    MovieId = movie.Id,
                    //    ActorId = actorId
                    //});
                    await _movieActorRepository.CreateAsync(new MovieActor
                    {
                        MovieId = movie.Id,
                        ActorId = actorId
                    });
                }
            }
            //_context.SaveChanges();
            await _movieActorRepository.CommitAsync();

            if (SubImgs.Any())
            {
                var movieSubImgs = await _movieSubImgRepository.GetAsync(e => e.MovieId == movie.Id);

                foreach (var item in SubImgs)
                {
                    var newFileName = Guid.NewGuid().ToString().Substring(0, 7) + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(item.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\movie_imgs\\sub_images", newFileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    //_context.MovieSubImgs.Add(new()
                    //{
                    //    MovieId = movie.Id,
                    //    SubImg = newFileName,
                    //});

                    await _movieSubImgRepository.CreateAsync(new()
                    {
                        MovieId = movie.Id,
                        SubImg = newFileName,
                    });
                }

                foreach (var item in movieSubImgs)
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\movie_imgs\\sub_images", item.SubImg);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                //_context.MovieSubImgs.RemoveRange(movieSubImgs);
                _movieSubImgRepository.DeleteRange(movieSubImgs);

                //_context.SaveChanges();
                await _movieSubImgRepository.CommitAsync();
            }

            TempData["success-notification"] = "Updated Movie Successfully";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteImg([FromRoute] int id, [FromQuery] int movieImgId)
        {
            //var movieSubImg = _context.MovieSubImgs.Find(movieImgId);
            var movieSubImg = await _movieSubImgRepository.GetOneAsync(e => e.Id == movieImgId);

            if (movieSubImg is null) return NotFound();

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\movie_imgs\\sub_images", movieSubImg.SubImg);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            //_context.MovieSubImgs.Remove(movieSubImg);
            //_context.SaveChanges();

            _movieSubImgRepository.Delete(movieSubImg);
            await _movieSubImgRepository.CommitAsync();

            TempData["success-notification"] = "Deleted One Of SubImgs Movie Successfully";

            return RedirectToAction(nameof(Edit), new { id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            //var movie = _context.Movies.Find(id);
            var movie = await _movieRepository.GetOneAsync(e => e.Id == id);

            if (movie is null) return NotFound();

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\movie_imgs", movie.MainImg);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            var movieSubImgs = await _movieSubImgRepository.GetAsync(e => e.MovieId == movie.Id);
            foreach (var item in movieSubImgs)
            {
                var oldSubImgsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\movie_imgs\\sub_images", item.SubImg);
                if (System.IO.File.Exists(oldSubImgsFilePath))
                {
                    System.IO.File.Delete(oldSubImgsFilePath);
                }
            }

            //var movieActors = _context.MovieActors.Where(ma => ma.MovieId == id);
            //_context.MovieActors.RemoveRange(movieActors);
            var movieActors = await _movieActorRepository.GetAsync(ma => ma.MovieId == id);
            _movieActorRepository.DeleteRange(movieActors);

            _movieSubImgRepository.DeleteRange(movieSubImgs);
            _movieRepository.Delete(movie);

            //_context.SaveChanges();
            await _movieRepository.CommitAsync();

            TempData["success-notification"] = "Deleted Movie Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
