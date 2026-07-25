using CatalogAPI.Domain.Entities;
using Xunit;

namespace CatalogAPI.Tests
{
    public class UserGameTests
    {
        [Fact]
        public void Construtor_DeveAtribuirTodosOsCampos()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            var userGame = new UserGame(id, userId, gameId, 199.90m);

            Assert.Equal(id, userGame.Id);
            Assert.Equal(userId, userGame.UserId);
            Assert.Equal(gameId, userGame.GameId);
            Assert.Equal(199.90m, userGame.Price);
        }

        [Fact]
        public void Construtor_DeveDefinirDataDeCompraEmUtc()
        {
            var antes = DateTime.UtcNow;

            var userGame = new UserGame(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                59.90m);

            Assert.InRange(userGame.PurchasedAt, antes, DateTime.UtcNow);
        }
    }

    public class GameTests
    {
        [Fact]
        public void NovoGame_DeveTerValoresPadraoSeguros()
        {
            var game = new Game();

            Assert.Equal(string.Empty, game.Name);
            Assert.Equal(string.Empty, game.Description);
            Assert.Equal(string.Empty, game.CoverUrl);
            Assert.True(game.CreatedAt <= DateTime.UtcNow);
        }
    }
}
