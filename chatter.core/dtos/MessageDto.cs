using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.enums;

namespace chatter.core.dtos
{
    public class MessageDto
    {
       public string ReceiverPhoneNumber {get;set;} = null!;
        public string Context {get;set;} = null!; 
    }
}