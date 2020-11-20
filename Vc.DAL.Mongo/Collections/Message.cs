using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.DAL.Mongo.Collections
{
    public class Message : AbstractCollection
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string MessageBody { get; set; }
        public DateTime DateSent { get; set; }
        public string SenderId { get; set; }
        public string RoomId { get; set; }
    }
}
