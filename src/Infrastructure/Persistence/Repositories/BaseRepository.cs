using Application.Common.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<T>(ApplicationDbContext context) : IBaseRepository<T>
    where T : class
{
    public void Add(T entity)
    {
        context.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
        context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        context.Set<T>().Remove(entity);
    }
}
