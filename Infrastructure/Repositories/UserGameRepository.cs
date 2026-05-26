using CatalogAPI.Domain.Entities;
using CatalogAPI.Infrastructure.Data;
using MongoDB.Driver;

namespace CatalogAPI.Infrastructure.Repositories
{
    public class UserGameRepository(AppDbContext context) : IUserGameRepository
    {
        private readonly AppDbContext _context = context;

        public async Task AddAsync(UserGame user)
        {
            await _context.UserGames.InsertOneAsync(user);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid gameId)
        {
            return await _context.UserGames
                .Find(x => x.UserId == userId && x.GameId == gameId)
                .AnyAsync();
        }

        public async Task<List<UserGame>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserGames
                .Find(x => x.UserId == userId)
                .ToListAsync();
        }
    }
}