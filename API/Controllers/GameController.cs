using CatalogAPI.API.Controllers.Models;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Infrastructure.Repositories;
using CatalogAPI.Infrastructure.Search;
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
        private readonly IGameSearchService _searchService;

        public GameController(
            IGameRepository gameRepository,
            IUserGameRepository userGameRepository,
            IDistributedCache cache,
            IGameSearchService searchService)
        {
            _gameRepository = gameRepository;
            _userGameRepository = userGameRepository;
            _cache = cache;
            _searchService = searchService;
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

            await _searchService.IndexAsync(game);

            await _cache.RemoveAsync("games:list");

            return Created(string.Empty, game);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateGame(Guid id, UpdateGameRequest request)
        {
            var game = await _gameRepository.GetByIdAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            game.Name = request.Name;
            game.Description = request.Description;
            game.Price = request.Price;
            game.CoverUrl = request.CoverUrl;

            await _gameRepository.UpdateAsync(game);

            await _searchService.IndexAsync(game);

            await _cache.RemoveAsync("games:list");

            return Ok(game);
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchGames([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new
                {
                    error = "O parâmetro 'q' é obrigatório."
                });
            }

            var results = await _searchService.SearchAsync(q);

            return Ok(new
            {
                source = "opensearch",
                query = q,
                data = results
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("reindex")]
        public async Task<IActionResult> ReindexGames()
        {
            var games = await _gameRepository.GetAllAsync();

            var indexed = await _searchService.ReindexAllAsync(games);

            return Ok(new
            {
                total = games.Count,
                indexed
            });
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