using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.attributes;
using chatter.core.interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace chatter.core.entities
{
    [CollectionName("Contacts")]
    public class Contact : IMongoCollection
    {
        public string CreatorPhoneNumber {get;set;} = null!;
        public string UserName {get;set;} = null!;
        public string PhoneNumber {get;set;} = null!;
    }
}