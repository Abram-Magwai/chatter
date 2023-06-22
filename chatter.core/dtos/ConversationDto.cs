using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.entities;
using chatter.core.models;

namespace chatter.core.dtos
{
    public class ConversationDto
    {
        public string ContactName {get;set;} = null!;
        public List<Chat> Messages {get;set;}
        public string LastSeen {get;set;} = null!;
        public ConversationDto()
        {
            Messages = new();
        }
    }
}