using chatter.core.dtos;
using chatter.core.entities;
using chatter.core.enums;
using chatter.core.interfaces;
using chatter.core.models;

namespace chatter.core.services
{
    public class MessageService : IMessageService
    {
        private readonly IMongoRepository<Message> _messageCollection;
        private readonly IMongoRepository<Contact> _contactCollection;
        private readonly IUserService _userService;
        public MessageService(
            IMongoRepository<Message> messageCollection,
            IMongoRepository<Contact> contactCollection,
            IUserService userService
        )
        {
            _messageCollection = messageCollection;
            _contactCollection = contactCollection;
            _userService = userService;
        }
        public async Task<ConversationDto> GetConversationAsync(string userPhoneNumber, string ContactPhoneNumber)
        {
            var contact = await _userService.GetUserByPhone(ContactPhoneNumber);
            var user = await _userService.GetUserByPhone(userPhoneNumber);

            var messageList = (await Task.Run(() => _messageCollection.AsQueryable().Where(message =>
            ((message.SenderPhoneNumber == user.PhoneNumber) && (message.ReceiverPhoneNumber == contact.PhoneNumber)) ||
            ((message.SenderPhoneNumber == contact.PhoneNumber && message.ReceiverPhoneNumber == user.PhoneNumber))
            ))).ToList();

            foreach (var message in messageList)
            {
                if ((message.Status == MessageStatusEnums.Delivered.ToString()) && (message.ReceiverPhoneNumber == user.UserName))
                {
                    message.Status = MessageStatusEnums.Read.ToString();
                    await _messageCollection.UpdateAsync(message.Id, message);
                }
            }

            var myContact = _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == userPhoneNumber) && (c.PhoneNumber == contact.PhoneNumber)).FirstOrDefault();
            string contactName = myContact == null ? contact.PhoneNumber : myContact.UserName;
            var lastSeen = string.Empty;
            if (contact.IsOnline)
                lastSeen = "Online";
            else
            {
                if (contact.LastSeen.Year == DateTime.Now.Year)
                {
                    if (contact.LastSeen.ToString("d") == DateTime.Today.ToString("d"))
                        lastSeen = contact.LastSeen.AddHours(2).Hour + ":" + contact.LastSeen.Minute;
                    else if (contact.LastSeen.ToString("d") == DateTime.Today.AddDays(-1).ToString("d"))
                        lastSeen = "Yesterday " + contact.LastSeen.AddHours(2).Hour + ":" + contact.LastSeen.Minute;
                    else
                        lastSeen = contact.LastSeen.ToString("dd MMMM") + " " + contact.LastSeen.AddHours(2).Hour + ":" + contact.LastSeen.Minute;
                }
                else
                    lastSeen = contact.LastSeen.ToString("d") + " " + contact.LastSeen.AddHours(2).Hour + ":" + contact.LastSeen.Minute;
            }

            List<Chat> chats = new();
            foreach (var message in messageList)
            {
                string messageDay = message.TimeSent.ToString("d");
                string yesterday = DateTime.Now.AddDays(-1).ToString("d");
                string today = DateTime.Now.ToString("d");

                if (messageDay == today)
                    messageDay = "Today";
                else if (messageDay == yesterday)
                    messageDay = "Yesterday";

                var chat = new Chat
                {
                    Date = messageDay,
                    Context = message.Context,
                    Type = message.ReceiverPhoneNumber == userPhoneNumber ? MessageTypesEnums.Received.ToString() : MessageTypesEnums.Sent.ToString(),
                    Time = message.TimeSent.AddHours(2).Hour + ":" + message.TimeSent.Minute
                };
                if (chat.Type == MessageTypesEnums.Sent.ToString())
                    chat.Status = message.Status;
                chats.Add(chat);
            }
            chats.Sort();
            ConversationDto conversation = new()
            {
                ContactName = contactName,
                Messages = chats,
                LastSeen = lastSeen
            };
            return conversation;
        }
        public async Task<Message> SendMessage(Message message)
        {
            await _messageCollection.CreateAsync(message);
            return message;
        }
        public async Task<List<LatestMessageDto>> GetRecentMessages(string userPhoneNumber)
        {
            var messageList = (await Task.Run(() => _messageCollection.AsQueryable().Where(message =>
            message.SenderPhoneNumber == userPhoneNumber ||
            message.ReceiverPhoneNumber == userPhoneNumber
            ))).ToList();
            List<LatestMessageDto> latestMessageDtos = new();
            foreach (var message in messageList)
            {
                var latestMessageDto = new LatestMessageDto();
                if (message.SenderPhoneNumber == userPhoneNumber)
                { //user sent message
                    var mycontact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == userPhoneNumber) && (c.PhoneNumber == message.ReceiverPhoneNumber)).FirstOrDefault());
                    latestMessageDto.ContactUserName = mycontact == null ? message.ReceiverPhoneNumber : mycontact.UserName;
                    latestMessageDto.Status = message.Status;
                }
                else
                {
                    var mycontact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == userPhoneNumber) && (c.PhoneNumber == message.SenderPhoneNumber)).FirstOrDefault());
                    if (mycontact == null)
                        latestMessageDto.ContactUserName = message.SenderPhoneNumber;
                    else
                        latestMessageDto.ContactUserName = mycontact.UserName;
                }
                if (message.Status == MessageStatusEnums.Sent.ToString())
                {
                    if ((message.Status == MessageStatusEnums.Sent.ToString()) && (message.ReceiverPhoneNumber == userPhoneNumber))
                    {
                        message.Status = MessageStatusEnums.Delivered.ToString();
                        message.TimeDelivered = DateTime.Now;
                    }
                }
                //update message
                await _messageCollection.UpdateAsync(message.Id, message);

                latestMessageDto.Context = message.Context;
                latestMessageDto.Time = message.TimeSent.AddHours(2).Hour + ":" + message.TimeSent.Minute;
                if (latestMessageDtos.Count > 0)
                {
                    var contact = latestMessageDtos.AsQueryable().Where(message => message.ContactUserName == latestMessageDto.ContactUserName).FirstOrDefault();
                    if (contact == null)
                        latestMessageDtos.Add(latestMessageDto);
                    else
                    {
                        latestMessageDtos.Remove(contact);
                        latestMessageDtos.Add(latestMessageDto);
                    }
                }
                else
                    latestMessageDtos.Add(latestMessageDto);
            }
            latestMessageDtos = await GetUnreadMessages(userPhoneNumber, latestMessageDtos);
            latestMessageDtos.Sort();
            return latestMessageDtos;
        }
        private async Task<List<LatestMessageDto>> GetUnreadMessages(string userPhoneNumber, List<LatestMessageDto> latestMessageDtos)
        {
            foreach (var latestMessage in latestMessageDtos)
            {
                var contact = _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == userPhoneNumber) && (c.UserName == latestMessage.ContactUserName)).FirstOrDefault();
                var contactPhone = contact == null ? latestMessage.ContactUserName : contact.PhoneNumber;
                var unreadMessages = await Task.Run(() => _messageCollection.AsQueryable().Count(c => (c.ReceiverPhoneNumber == userPhoneNumber) && ((c.SenderPhoneNumber == contactPhone) && (c.Status == MessageStatusEnums.Delivered.ToString()))));
                latestMessage.UnreadMessages = unreadMessages;
            }
            return latestMessageDtos;
        }
    }
}