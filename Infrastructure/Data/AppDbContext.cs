using CatalogAPI.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CatalogAPI.Infrastructure.Data
{
    public class AppDbContext
    {
        private readonly IMongoDatabase _database;

        public AppDbContext(IOptions<MongoDbSettings> settings)
        {
            var CONNECTION_STRING = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            var DATABASE_NAME = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");

            if(CONNECTION_STRING == null || DATABASE_NAME == null)
            {
                throw new Exception("MongoDB configuration is missing. Please set environment variables MONGODB_CONNECTION_STRING and MONGODB_DATABASE_NAME");
            }

            var client = new MongoClient(CONNECTION_STRING);
            _database = client.GetDatabase(DATABASE_NAME);
        }

        public IMongoCollection<UserGame> UserGames =>
            _database.GetCollection<UserGame>("usergames");

        public IMongoCollection<Game> Games =>
            _database.GetCollection<Game>("games");
    }
}
