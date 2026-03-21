using CatalogAPI.Domain.Entities;

namespace CatalogAPI.Infrastructure.Repositories
{
    public interface IUserGameRepository
    {
        Task AddAsync(UserGame userGame);
    }
}