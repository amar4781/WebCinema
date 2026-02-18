namespace WebCinema.Models
{
    public class ApplicationUserOTP
    {
        public int Id { get; set; }
        public string OTP { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get; set; }
        public DateTime ExpiredAt { get; set; } = DateTime.UtcNow.AddHours(3);
        public bool IsValid => ExpiredAt > DateTime.UtcNow && !IsUsed;
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
