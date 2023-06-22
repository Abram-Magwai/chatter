using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Models;
using chatter.core.attributes;
using chatter.core.interfaces;

namespace chatter.core.entities
{
    [CollectionName("ApplicationUsers")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public DateTime LastSeen {get;set;} = DateTime.Now;
        public bool IsOnline {get;set;} = false;
        public string SignalRConnectionId {get;set;} = null!;
    }
}