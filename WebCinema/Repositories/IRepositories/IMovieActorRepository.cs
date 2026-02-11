namespace WebCinema.Repositories.IRepositories
{
    public interface IMovieActorRepository : IRepository<MovieActor>
    {
        void DeleteRange(List<MovieActor> movieActorList);
    }
}
