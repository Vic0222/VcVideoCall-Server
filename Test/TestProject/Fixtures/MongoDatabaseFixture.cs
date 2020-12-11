using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vc.DAL.Mongo;
using Vc.DAL.Mongo.Collections;

namespace TestProject.Fixtures
{
    public class MongoDatabaseFixture : IDisposable
    {
        private MongoClient _defaultMongoClient;
        private readonly MongoDatabaseSetting _mongoDatabaseSetting;

        public MongoDatabaseFixture(MongoDatabaseSetting mongoDatabaseSetting)
        {
            _defaultMongoClient = new MongoClient(mongoDatabaseSetting.ConnectionString);
            _mongoDatabaseSetting = mongoDatabaseSetting;

            Seed(_defaultMongoClient, mongoDatabaseSetting.Database);
            
        }

        public void Seed(MongoClient defaultMongoClient, string databaseName)
        {
            var database = defaultMongoClient.GetDatabase(databaseName);
            var users = database.GetCollection<User>("users");

            var user1 = new User() { Id = "wUnSd3SPUhWngjOsXK83EkPVFyW2", Username = "Vic" };
            users.InsertOne(user1);

            var user2 = new User() { Id = "88Ne5eX2C6ZMsG0wZCATJnEMFWH3", Username = "Alb" };
            users.InsertOne(user2);

            var room = new Room() { Id = "5fbcfc82231676fa807c5d3e", Name = "wUnSd3SPUhWngjOsXK83EkPVFyW2-88Ne5eX2C6ZMsG0wZCATJnEMFWH3", Type = 1 };
            room.RoomUsers.Add(new RoomUser() { UserId = "wUnSd3SPUhWngjOsXK83EkPVFyW2", Nickname = "Vic", PhotoUrl = "https://lh3.googleusercontent.com/-9-6MMBQxGMg/AAAAAAAAAAI/AAAAAAAAAAA/AMZuuck8zaDNdXmlNSHMIqfhcWQVZNMO8Q/s96-c/photo.jpg" });
            room.RoomUsers.Add(new RoomUser() { UserId = "88Ne5eX2C6ZMsG0wZCATJnEMFWH3", Nickname = "Alb", PhotoUrl = "https://lh3.googleusercontent.com/-WOo0vt6zQcs/AAAAAAAAAAI/AAAAAAAAAAA/AMZuucl4khkgDvVRQS_7iIPPLiVW0QPPag/s96-c/photo.jpg" });

            var rooms = database.GetCollection<Room>("rooms");
            rooms.InsertOne(room);
        }

        public void Dispose()
        {
            _defaultMongoClient.DropDatabase(_mongoDatabaseSetting.Database);
        }
    }
}
