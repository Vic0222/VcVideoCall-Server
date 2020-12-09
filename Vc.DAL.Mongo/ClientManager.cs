using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace Vc.DAL.Mongo
{
    public class ClientManager
    {
        private readonly MongoDatabaseSetting _mongoDatabaseSetting;
        private MongoClient _defaultMongoClient;

        public ClientManager(IOptions<MongoDatabaseSetting> mongoDatabaseSetting)
        {
            _mongoDatabaseSetting = mongoDatabaseSetting?.Value;
            RenewMongoClient();
        }

        public void RenewMongoClient()
        {
            _defaultMongoClient = new MongoClient(_mongoDatabaseSetting.ConnectionString);
        }

        public MongoClient DefaultMongoClient { get => _defaultMongoClient;  }

        public IMongoDatabase GetDatabase()
        {
            return _defaultMongoClient.GetDatabase(_mongoDatabaseSetting.Database ?? "");
        }
    }
}
