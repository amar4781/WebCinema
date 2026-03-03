namespace WebCinema.ViewModels
{
    public class UsersVM
    {
        public IEnumerable<UserWithRolesVM> ApplicationUsers { get; set; }
        public int CurrentPage { get; set; }
        public double TotalPages { get; set; }
    }
}
