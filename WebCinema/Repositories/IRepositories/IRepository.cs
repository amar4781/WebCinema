using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace WebCinema.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task CreateAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true);

        Task<T?> GetOneAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true);

        Task<int> CommitAsync();
    }
}
