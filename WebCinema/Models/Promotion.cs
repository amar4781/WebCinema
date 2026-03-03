namespace WebCinema.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Code is required")]
        [StringLength(20, ErrorMessage = "Max length is 20 characters")]
        public string Code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Discount is required")]
        [Range(0.01, 100, ErrorMessage = "Discount must be between 0.01 and 100")]
        public decimal Discount { get; set; }
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        [Required(ErrorMessage = "Movie is required")]
        public int? MovieId { get; set; }
        public Movie? Movie { get; set; }
        [Required(ErrorMessage = "Max usage is required")]
        [Range(1, 1000, ErrorMessage = "Max usage must be at least 1")]
        public int MaxUsage { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [DataType(DataType.DateTime)]
        public DateTime ExpiredAt { get; set; } = DateTime.UtcNow.AddDays(30);
        public bool IsValid => MaxUsage >= 1 && ExpiredAt > DateTime.UtcNow;
    }
}
