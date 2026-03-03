namespace WebCinema.ViewModels
{
    public class PromotionUpdateResponseVM
    {
        public Promotion Promotion { get; set; } = new();
        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();
    }
}
