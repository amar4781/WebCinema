namespace WebCinema.ViewModels
{
    public class EditUserVM
    {
        public string Id { get; set; } = null!;

        [Required]
        public string FName { get; set; } = null!;

        [Required]
        public string LName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public List<string> Roles { get; set; } = new();
        public string SelectedRole { get; set; } = null!;
    }
}
