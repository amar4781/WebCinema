namespace WebCinema.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Movie name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description can't exceed 2000 characters")]
        public string? Description { get; set; }

        public string MainImg { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Price must be between 0.01 and 9999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Movie date is required")]
        [DataType(DataType.Date)]
        public DateTime DateTime { get; set; }

        public bool Status { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a cinema")]
        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } = default!;

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
        public ICollection<MovieSubImg> MovieSubImgs { get; set; } = new List<MovieSubImg>();
    }

}
