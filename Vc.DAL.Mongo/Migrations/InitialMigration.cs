using MongoDB.Driver;
using MongoDBMigrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.DAL.Mongo.Migrations
{
    public class InitialMigration : IMigration
    {
        public MongoDBMigrations.Version Version => new MongoDBMigrations.Version(0,1,0);

        public string Name => "Initial Migration";

        public void Up(IMongoDatabase database)
        {
            database.CreateCollection("room");
        }

        public void Down(IMongoDatabase database)
        {
            database.DropCollection("room");
        }

        
    }
}
