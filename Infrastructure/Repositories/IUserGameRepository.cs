using CatalogAPI.Domain.Entities;

namespace CatalogAPI.Infrastructure.Repositories
{
    public interface IUserGameRepository
    {
        Task AddAsync(UserGame userGame);
        Task<bool> ExistsAsync(Guid userId, Guid gameId);
        Task<List<UserGame>> GetByUserIdAsync(Guid userId);
    }
}