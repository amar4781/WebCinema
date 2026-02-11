namespace WebCinema.Repositories
{
    public class MovieSubImgRepository : Repository<MovieSubImg>, IMovieSubImgRepository
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        public MovieSubImgRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void DeleteRange(List<MovieSubImg> subImgList)
        {
            _context.MovieSubImgs.RemoveRange(subImgList);
        }
    }
}
