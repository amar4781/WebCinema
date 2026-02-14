namespace WebCinema.Models
{
    public class MovieSubImg
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Sub image is required")]
        public string SubImg { get; set; } = string.Empty;
        [Required(ErrorMessage = "Movie is required")]
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = default!;
    }
}
