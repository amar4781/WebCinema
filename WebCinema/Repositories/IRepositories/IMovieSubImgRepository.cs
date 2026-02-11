namespace WebCinema.Repositories.IRepositories
{
    public interface IMovieSubImgRepository : IRepository<MovieSubImg>
    {
        void DeleteRange(List<MovieSubImg> subImgList);
    }
}
