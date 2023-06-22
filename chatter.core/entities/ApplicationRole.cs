using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Models;
using chatter.core.attributes;

namespace chatter.core.entities
{
    [CollectionName("ApplicationRoles")]
    public class ApplicationRole : MongoIdentityRole<Guid>
    {
        
    }
}