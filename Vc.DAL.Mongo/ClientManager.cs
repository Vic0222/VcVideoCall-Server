using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace Vc.DAL.Mongo
{
    public class ClientManager
    {
        private readonly MongoDatabaseSetting _mongoDatabaseSetting;

        public ClientManager(IOptions<MongoDatabaseSetting> mongoDatabaseSetting)
        {
            _mongoDatabaseSetting = mongoDatabaseSetting?.Value;
        }
        public MongoClient GetMongoClient()
        {
            return new MongoClient(_mongoDatabaseSetting.ConnectionString);
        }

        public IMongoDatabase GetDatabase()
        {
            var mongoClient = GetMongoClient();
            return mongoClient.GetDatabase(_mongoDatabaseSetting.Database ?? "");
        }
    }
}
