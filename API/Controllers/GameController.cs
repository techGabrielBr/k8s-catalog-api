using CatalogAPI.API.Controllers.Models;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

namespace CatalogAPI.API.Controllers
{
    [ApiController]
    [Route("/games")]
    public class GameController : ControllerBase
    {
        private readonly IGameRepository _gameRepository;
        private readonly IUserGameRepository _userGameRepository;
        private readonly IDistributedCache _cache;

        public GameController(
            IGameRepository gameRepository,
            IUserGameRepository userGameRepository,
            IDistributedCache cache)
        {
            _gameRepository = gameRepository;
            _userGameRepository = userGameRepository;
            _cache = cache;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateGame(CreateGameRequest request)
        {
            var game = new Game
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CoverUrl = request.CoverUrl
            };

            await _gameRepository.CreateAsync(game);

            await _cache.RemoveAsync("games:list");

            return Created(string.Empty, game);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetGames()
        {
            const string cacheKey = "games:list";

            var cachedGames = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedGames))
            {
                var gamesFromCache =
                    JsonSerializer.Deserialize<List<Game>>(cachedGames);

                return Ok(new
                {
                    source = "redis",
                    data = gamesFromCache
                });
            }

            var games = await _gameRepository.GetAllAsync();

            var serialized = JsonSerializer.Serialize(games);

            await _cache.SetStringAsync(
                cacheKey,
                serialized,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(5)
                });

            return Ok(new
            {
                source = "mongodb",
                data = games
            });
        }

        [Authorize]
        [HttpGet("my-catalog")]
        public async Task<IActionResult> GetMyCatalog()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var catalog = await _userGameRepository
                .GetByUserIdAsync(Guid.Parse(userId));

            return Ok(catalog);
        }
    }
}