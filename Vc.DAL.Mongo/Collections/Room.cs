using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.DAL.Mongo.Collections
{
    public class Room : AbstractCollection
    {
        public Room()
        {
            RoomUsers = new List<RoomUser>();
        }

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string Name { get; set; }
        public byte Type { get; set; }
        public List<RoomUser> RoomUsers { get; set; }
    }
}
