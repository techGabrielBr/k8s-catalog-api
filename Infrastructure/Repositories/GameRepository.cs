using CatalogAPI.Domain.Entities;
using CatalogAPI.Infrastructure.Data;
using MongoDB.Driver;

namespace CatalogAPI.Infrastructure.Repositories
{
    public class GameRepository(AppDbContext context) : IGameRepository
    {
        private readonly AppDbContext _context = context;

        public async Task CreateAsync(Game game)
        {
            await _context.Games.InsertOneAsync(game);
        }

        public async Task<List<Game>> GetAllAsync()
        {
            return await _context.Games
                .Find(_ => true)
                .ToListAsync();
        }

        public async Task<Game?> GetByIdAsync(Guid id)
        {
            return await _context.Games
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Game game)
        {
            await _context.Games.ReplaceOneAsync(
                x => x.Id == game.Id,
                game
            );
        }
    }
}