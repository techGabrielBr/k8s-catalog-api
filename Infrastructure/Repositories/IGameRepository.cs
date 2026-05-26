using CatalogAPI.Domain.Entities;

namespace CatalogAPI.Infrastructure.Repositories
{
    public interface IGameRepository
    {
        Task CreateAsync(Game game);

        Task<List<Game>> GetAllAsync();

        Task<Game?> GetByIdAsync(Guid id);
    }
}