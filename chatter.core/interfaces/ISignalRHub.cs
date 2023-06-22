using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.dtos;

namespace chatter.core.interfaces
{
    public interface ISignalRHub
    {
        Task SendToReceiver(MessageDto messageDto, string senderPhoneNumber);
        Task UpdateSentMessageStatus(string status, string senderName, string receiverName);
        Task SendMessage(MessageDto messageDto, string userPhoneNumber);
        Task UpdateOnLogin(string userPhoneNumber);
    }
}