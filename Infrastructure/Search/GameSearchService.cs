using CatalogAPI.Domain.Entities;
using OpenSearch.Client;

namespace CatalogAPI.Infrastructure.Search
{
    public class GameSearchService(
        IOpenSearchClient client,
        ILogger<GameSearchService> logger) : IGameSearchService
    {
        public const string IndexName = "games";

        private readonly IOpenSearchClient _client = client;
        private readonly ILogger<GameSearchService> _logger = logger;

        public async Task IndexAsync(Game game)
        {
            try
            {
                var response = await _client.IndexAsync(game, i => i
                    .Index(IndexName)
                    .Id(game.Id)
                );

                if (!response.IsValid)
                {
                    _logger.LogError(
                        "Falha ao indexar game {GameId} no OpenSearch: {Error}",
                        game.Id,
                        response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao indexar game {GameId} no OpenSearch", game.Id);
            }
        }

        public async Task<List<GameSearchResult>> SearchAsync(string query)
        {
            var response = await _client.SearchAsync<Game>(s => s
                .Index(IndexName)
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(g => g.Name, boost: 3)
                            .Field(g => g.Description)
                        )
                        .Query(query)
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
                .Size(20)
            );

            if (!response.IsValid)
            {
                _logger.LogError(
                    "Falha na busca do OpenSearch: {Error}",
                    response.DebugInformation);

                throw new Exception("Erro ao consultar o motor de busca.");
            }

            return response.Hits
                .Select(h => new GameSearchResult(
                    h.Source.Id,
                    h.Source.Name,
                    h.Source.Description,
                    h.Source.Price,
                    h.Source.CoverUrl,
                    h.Score
                ))
                .ToList();
        }

        public async Task<long> ReindexAllAsync(IEnumerable<Game> games)
        {
            var response = await _client.BulkAsync(b => b
                .Index(IndexName)
                .IndexMany(games, (op, game) => op.Id(game.Id))
            );

            if (response.Errors)
            {
                _logger.LogError(
                    "Falha ao reindexar games no OpenSearch: {Error}",
                    response.DebugInformation);
            }

            return response.Items.Count(i => i.IsValid);
        }
    }
}
