using CatalogAPI.Domain.Entities;

namespace CatalogAPI.Infrastructure.Search
{
    public interface IGameSearchService
    {
        Task IndexAsync(Game game);

        Task<List<GameSearchResult>> SearchAsync(string query);

        Task<long> ReindexAllAsync(IEnumerable<Game> games);
    }

    public record GameSearchResult(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        string CoverUrl,
        double? Score
    );
}
