using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatter.core.dtos
{
    public class ContactDto
    {
        public string ContactId {get;set;} = null!;
        public string UserName {get;set;} = null!;
        public string PhoneNumber {get;set;} = null!;
        public bool IsMember {get;set;}
    }
}