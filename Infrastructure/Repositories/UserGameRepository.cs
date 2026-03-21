using Microsoft.EntityFrameworkCore;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Infrastructure.Data;

namespace CatalogAPI.Infrastructure.Repositories
{
    public class UserGameRepository(AppDbContext context) : IUserGameRepository
    {
        private readonly AppDbContext _context = context;

        public async Task AddAsync(UserGame user)
        {
            _context.UserGames.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}