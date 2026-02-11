namespace WebCinema.Repositories
{
    public class MovieActorRepository : Repository<MovieActor>, IMovieActorRepository
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        public MovieActorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void DeleteRange(List<MovieActor> actorList)
        {
            _context.MovieActors.RemoveRange(actorList);
        }
    }
}
