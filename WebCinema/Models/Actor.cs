namespace WebCinema.Models
{
    public class Actor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Actor name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;
        public string Img { get; set; } = string.Empty;
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
