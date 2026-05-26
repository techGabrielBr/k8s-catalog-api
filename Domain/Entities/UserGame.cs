using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CatalogAPI.Domain.Entities
{
    public class UserGame
    {
        protected UserGame() { }

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

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid GameId { get; set; }

        public decimal Price { get; set; }

        public DateTime PurchasedAt { get; set; }
    }
}
