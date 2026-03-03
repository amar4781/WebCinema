namespace WebCinema.ViewModels
{
    public class UserWithRolesVM
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
    }
}
