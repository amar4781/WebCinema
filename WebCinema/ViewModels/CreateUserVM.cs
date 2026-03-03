namespace WebCinema.ViewModels
{
    public class CreateUserVM
    {
        [Required]
        public string FName { get; set; } = null!;

        [Required]
        public string LName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public List<string> Roles { get; set; } = new();
        public string SelectedRole { get; set; } = null!;
    }
}
