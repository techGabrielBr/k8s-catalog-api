namespace CatalogAPI.Domain.Entities
{
    public class UserGame
    {
        protected UserGame() { } // EF Core

        public UserGame(
                Guid id,
                Guid userId,
                Guid gameId,
                decimal price
            )
        {
            Id = id;
            UserId = userId;
            GameId = gameId;
            Price = price;
            PurchasedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public decimal Price { get; private set; }
        public DateTime PurchasedAt { get; private set; }
    }
}
