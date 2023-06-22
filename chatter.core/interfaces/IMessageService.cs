using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.dtos;
using chatter.core.entities;

namespace chatter.core.interfaces
{
    public interface IMessageService
    {
        Task<ConversationDto> GetConversationAsync(string userPhoneNumber, string ContactPhoneNumber);
        Task<Message> SendMessage(Message message);
        Task<List<LatestMessageDto>> GetRecentMessages(string userPhoneNumber);
    }
}