using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace chatter.core.interfaces
{
    public abstract class IMongoCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get;set;} = null!;
        public DateTime DateCreated {get;set;} = DateTime.Now;
    }
}