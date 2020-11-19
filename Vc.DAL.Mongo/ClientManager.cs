using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;

namespace Vc.DAL.Mongo
{
    public class ClientManager
    {
        private readonly IConfiguration _configuration;

        public ClientManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public MongoClient GetMongoClient()
        {
            return new MongoClient(_configuration.GetConnectionString("VcMongoDbConnectionString"));
        }

        public IMongoDatabase GetDatabase()
        {
            var mongoClient = GetMongoClient();
            return mongoClient.GetDatabase(_configuration.GetSection("Database")?.Value ?? "");
        }
    }
}
