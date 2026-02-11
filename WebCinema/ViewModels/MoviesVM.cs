namespace WebCinema.ViewModels
{
    public class MoviesVM
    {
        public IEnumerable<Movie> Movies { get; set; }
        public int CurrentPage { get; set; }
        public double TotalPages { get; set; }
    }
}
