namespace Application.Common.Interfaces.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}
