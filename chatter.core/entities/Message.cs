using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.attributes;
using chatter.core.enums;
using chatter.core.interfaces;

namespace chatter.core.entities
{
    [CollectionName("Messages")]
    public class Message : IMongoCollection
    {
        public string SenderPhoneNumber {get;set;} = null!;
        public string ReceiverPhoneNumber {get;set;} = null!;
        public DateTime TimeSent {get;set;}
        public DateTime? TimeDelivered {get;set;} = null;
        public string Status {get;set;} = null!;
        public string Context {get;set;} = null!;
    }
}