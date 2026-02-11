using Microsoft.Build.Tasks.Deployment.Bootstrapper;

namespace WebCinema.ViewModels
{
    public class MovieUpdateResponseVM
    {
        public Movie Movie { get; set; } 
        public IEnumerable<MovieSubImg> MovieSubImgs { get; set; } 
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Cinema> Cinemas { get; set; }
        public IEnumerable<Actor> Actors { get; set; }
        public List<int> SelectedActorIds { get; set; }
    }
}
