namespace WebCinema.ViewModels
{
    public class MovieCreateVM
    {
        //public Movie Movie { get; set; } 
        public IEnumerable<Category> Categories { get; set; } 
        public IEnumerable<Cinema> Cinemas { get; set; } 
        public IEnumerable<Actor> Actors { get; set; } 
        //public List<int> SelectedActorIds { get; set; } = new List<int>();
    }
}
