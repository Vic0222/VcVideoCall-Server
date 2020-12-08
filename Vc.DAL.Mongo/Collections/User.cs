using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.DAL.Mongo.Collections
{
    public class User: AbstractCollection
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string Username { get; set; }
        public string PhotoUrl { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
