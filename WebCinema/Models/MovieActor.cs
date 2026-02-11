namespace WebCinema.Models
{
    public class MovieActor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Movie is required")]
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = default!;
        [Required(ErrorMessage = "Actor is required")]
        public int ActorId { get; set; }
        public Actor Actor { get; set; } = default!;
    }
}
