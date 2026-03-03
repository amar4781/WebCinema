namespace WebCinema.ViewModels
{
    public class MovieWithRelatedVM
    {
        public Movie Movie { get; set; }
        public List<MovieSubImg> MovieSubImgs { get; set; }
        public List<Category> Categories { get; set; }
        public List<Movie> SameCategories { get; set; }
        public List<Movie> RelatedMovies { get; set; }
    }
}
