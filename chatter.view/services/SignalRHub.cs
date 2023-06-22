using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.dtos;
using chatter.core.entities;
using chatter.core.enums;
using chatter.core.interfaces;
using chatter.core.models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace chatter.view.services
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class SignalRHub : Hub, ISignalRHub
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _applicationUser;
        private readonly IMessageService _messageService;
        private readonly IProfileService _profileService;
        private readonly IMongoRepository<Contact> _contactCollection;
        private readonly IMongoRepository<Message> _messageCollection;
        public SignalRHub(IUserService userService, UserManager<ApplicationUser> applicationUser, IMessageService messageService, IProfileService profileService, IMongoRepository<Contact> contactCollection, IMongoRepository<Message> messageCollection)
        {
            _applicationUser = applicationUser;
            _userService = userService;
            _messageService = messageService;
            _profileService = profileService;
            _contactCollection = contactCollection;
            _messageCollection = messageCollection;
        }
        public async Task SendToReceiver(MessageDto messageDto, string senderPhoneNumber)
        {
            //get receiver phone number then if receiver is online then get signalrconnection id
            var user = await _userService.GetUserByPhone(senderPhoneNumber);
            ApplicationUser receiver;
            var myContact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == senderPhoneNumber) && ((c.UserName == messageDto.ReceiverPhoneNumber) || (c.PhoneNumber == messageDto.ReceiverPhoneNumber))).FirstOrDefault());
            if (myContact != null)
                receiver = await _userService.GetUserByPhone(myContact.PhoneNumber);
            else
                receiver = await _userService.GetUserByPhone(messageDto.ReceiverPhoneNumber);

            var receiverPhoneNumber = _profileService.GetContactPhoneNumberAsync(senderPhoneNumber, messageDto.ReceiverPhoneNumber).Result;
            var message = new Message
            {
                SenderPhoneNumber = senderPhoneNumber,
                ReceiverPhoneNumber = receiverPhoneNumber,
                TimeSent = DateTime.Now,
                Status = MessageStatusEnums.Sent.ToString(),
                Context = messageDto.Context
            };
            var newMessage = await _messageService.SendMessage(message);
            var chat = new Chat
            {
                SentBy = await GetSentByName(senderPhoneNumber, receiverPhoneNumber),
                ReceivedBy = myContact == null ? messageDto.ReceiverPhoneNumber : myContact.UserName,
                Date = newMessage.TimeSent.ToString("d"),
                Context = newMessage.Context,
                Type = MessageTypesEnums.Sent.ToString(),
                Status = newMessage.Status,
                Time = newMessage.TimeSent.Hour + ":" + newMessage.TimeSent.Minute
            };
            await Clients.Client(connectionId: receiver.SignalRConnectionId).SendAsync("ReceiveMessage", chat);
        }
        public async Task UpdateSentMessageStatus(string status, string senderName, string receiverName)
        {
            var myContact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == receiverName) && (c.PhoneNumber == senderName || c.UserName == senderName)).FirstOrDefault());
            var myContactPhoneNumber = myContact == null ? senderName : myContact.PhoneNumber;
            var creatorPhone = (myContact != null ? myContact.PhoneNumber : senderName);
            var receiverContact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == creatorPhone) && ((c.PhoneNumber == receiverName) || (c.UserName == receiverName))).FirstOrDefault());

            var sender = await _userService.GetUserByPhone(myContact != null ? myContact.PhoneNumber : senderName);
            var receiver = await Task.Run(() => _applicationUser.FindByNameAsync(receiverName));
            var messages = (await Task.Run(() => _messageCollection.AsQueryable().Where(message =>
            ((message.SenderPhoneNumber == myContactPhoneNumber) && (message.ReceiverPhoneNumber == receiverName)) ||
            ((message.SenderPhoneNumber == receiverName) && (message.ReceiverPhoneNumber == myContactPhoneNumber))
            ))).ToList();

            Chat chat = new();
            foreach (var message in messages)
            {
                if (message.Status != null && message.Status != MessageStatusEnums.Read.ToString())
                {
                    message.Status = status;
                    message.TimeDelivered = DateTime.Now;
                    await _messageCollection.UpdateAsync(message.Id, message);
                }
            }
            await Clients.Client(connectionId: sender!.SignalRConnectionId).SendAsync("UpdateSentChat", new { senderName = receiverContact == null ? receiverName : receiverContact.UserName, Status = status });
        }
        public async Task SendMessage(MessageDto messageDto, string userPhoneNumber)
        {
            //get receiver phone number then if receiver is online then get signalrconnection id
            var user = await _userService.GetUserByPhone(userPhoneNumber);
            ApplicationUser receiver;
            var myContact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == userPhoneNumber) && ((c.UserName == messageDto.ReceiverPhoneNumber) || (c.PhoneNumber == messageDto.ReceiverPhoneNumber))).FirstOrDefault());
            if (myContact != null)
                receiver = await _userService.GetUserByPhone(myContact.PhoneNumber);
            else
                receiver = await _userService.GetUserByPhone(messageDto.ReceiverPhoneNumber);

            var receiverPhoneNumber = _profileService.GetContactPhoneNumberAsync(userPhoneNumber, messageDto.ReceiverPhoneNumber).Result;
            var message = new Message
            {
                SenderPhoneNumber = userPhoneNumber,
                ReceiverPhoneNumber = receiverPhoneNumber,
                TimeSent = DateTime.Now,
                Status = MessageStatusEnums.Sent.ToString(),
                Context = messageDto.Context
            };
            var newMessage = await _messageService.SendMessage(message);
            var chat = new Chat

            {
                SentBy = await GetSentByName(userPhoneNumber, receiverPhoneNumber),
                ReceivedBy = myContact == null ? messageDto.ReceiverPhoneNumber : myContact.UserName,
                Date = newMessage.TimeSent.ToString("d"),
                Context = newMessage.Context,
                Type = MessageTypesEnums.Sent.ToString(),
                Status = newMessage.Status,
                Time = newMessage.TimeSent.Hour + ":" + newMessage.TimeSent.Minute
            };
            await Clients.Client(connectionId: user.SignalRConnectionId).SendAsync("ReceiveMessage", chat);
            chat.Type = MessageTypesEnums.Received.ToString();
            await Clients.Client(connectionId: receiver.SignalRConnectionId).SendAsync("ReceiveMessage", chat);
        }
        private async Task<string> GetSentByName(string userPhoneNumber, string receiverPhoneNumber)
        {
            var myContact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == receiverPhoneNumber) && (c.PhoneNumber == userPhoneNumber)).FirstOrDefault());
            return myContact == null ? userPhoneNumber : myContact.UserName;
        }
        public async Task UpdateOnLogin(string userPhoneNumber)
        {
            var recentMessages = await _messageService.GetRecentMessages(userPhoneNumber);
            foreach (var recentMessage in recentMessages)
            {
                if (recentMessage.UnreadMessages >= 1)
                {
                    var myContact = _contactCollection.AsQueryable().Where(c => c.CreatorPhoneNumber == userPhoneNumber && ((c.UserName == recentMessage.ContactUserName) || (c.UserName == recentMessage.ContactUserName))).FirstOrDefault();
                    var senderName = myContact == null ? recentMessage.ContactUserName : myContact.PhoneNumber;
                    var sender = await _userService.GetUserByPhone(senderName);

                    var deliveredMessages = (await Task.Run(() => _messageCollection.AsQueryable().Where(message =>
                    (((message.SenderPhoneNumber == userPhoneNumber) && (message.ReceiverPhoneNumber == senderName)) ||
                    ((message.SenderPhoneNumber == senderName) && (message.ReceiverPhoneNumber == userPhoneNumber))
                    ) && (message.Status == MessageStatusEnums.Delivered.ToString())))).ToList();

                    foreach (var message in deliveredMessages)
                        await UpdateSentMessageStatus(MessageStatusEnums.Delivered.ToString(), senderName, userPhoneNumber);
                }
            }
        }
        public async Task UpdateMessage(string from, string to, string status, string messageType)
        {
            var myContact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == to) && (c.PhoneNumber == from || c.UserName == from)).FirstOrDefault());
            var myContactPhoneNumber = myContact == null ? from : myContact.PhoneNumber;

            var sender = await _userService.GetUserByPhone(myContact == null ? from : myContact.PhoneNumber);
            var receiver = await Task.Run(() => _applicationUser.FindByNameAsync(to));
            if (receiver == null)
            {
                var contact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == from) && (c.PhoneNumber == to || c.UserName == to)).FirstOrDefault());
                receiver = await Task.Run(() => _applicationUser.FindByNameAsync(contact!.PhoneNumber));
            }
            var messages = (await Task.Run(() => _messageCollection.AsQueryable().Where(message =>
            ((message.SenderPhoneNumber == myContactPhoneNumber) && (message.ReceiverPhoneNumber == receiver!.UserName)) ||
            ((message.SenderPhoneNumber == receiver!.UserName) && (message.ReceiverPhoneNumber == myContactPhoneNumber))
            ))).ToList();


            var receiverName = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == sender.UserName) && (c.PhoneNumber == to)).FirstOrDefault());
            Chat chat = new();
            foreach (var message in messages)
            {
                if (message.Status != null || message.Status != MessageStatusEnums.Read.ToString())
                {
                    message.Status = status;
                    message.TimeDelivered = DateTime.Now;
                    await _messageCollection.UpdateAsync(message.Id, message);
                    chat.Context = message.Context;
                    chat.Date = message.DateCreated.ToString();
                    chat.Status = message.Status;
                    chat.ReceivedBy = receiverName != null ? receiverName!.UserName : to;
                    chat.SentBy = from;
                    chat.Type = MessageTypesEnums.Received.ToString();
                    chat.Time = message.TimeSent.Hour + ":" + message.TimeSent.Minute;
                }
            }

            await Clients.Client(connectionId: receiver!.SignalRConnectionId).SendAsync("UpdateChat", chat);
            await Task.Run(() => chat.Type = messageType == MessageTypesEnums.Sent.ToString() ? MessageTypesEnums.Received.ToString() : MessageTypesEnums.Sent.ToString());
            await Clients.Client(connectionId: sender.SignalRConnectionId).SendAsync("UpdateChat", chat);
        }
        public async Task UpdateSentMessage(string from, string to, string status)
        {
            var myContact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == to) && (c.PhoneNumber == from || c.UserName == from)).FirstOrDefault());
            var myContactPhoneNumber = myContact == null ? from : myContact.PhoneNumber;

            var sender = await _userService.GetUserByPhone(myContact != null ? myContact.PhoneNumber : from);
            var receiver = await Task.Run(() => _applicationUser.FindByNameAsync(to));
            var messages = (await Task.Run(() => _messageCollection.AsQueryable().Where(message =>
            ((message.SenderPhoneNumber == myContactPhoneNumber) && (message.ReceiverPhoneNumber == to)) ||
            ((message.SenderPhoneNumber == to) && (message.ReceiverPhoneNumber == myContactPhoneNumber))
            ))).ToList();

            Chat chat = new();
            foreach (var message in messages)
            {
                if (message.Status != null || message.Status != MessageStatusEnums.Read.ToString())
                {
                    message.Status = status;
                    message.TimeDelivered = DateTime.Now;
                    await _messageCollection.UpdateAsync(message.Id, message);
                    chat.Context = message.Context;
                    chat.Date = message.DateCreated.ToString();
                    chat.Status = status;
                    chat.ReceivedBy = myContact != null ? myContact!.UserName : to;
                    chat.SentBy = from;
                    chat.Type = MessageTypesEnums.Sent.ToString();
                    chat.Time = message.TimeSent.Hour + ":" + message.TimeSent.Minute;
                }
            }
            var connectionId = (chat.SentBy == from ? receiver : sender)!.SignalRConnectionId;
            await Clients.Client(connectionId: connectionId).SendAsync("UpdateReceivedChat", chat);
        }
        public async Task UpdateReceivedMessage(string from, string to, string status)
        {
            var myContact = await Task.Run(() => _contactCollection.AsQueryable().Where(c => (c.CreatorPhoneNumber == from) && (c.PhoneNumber == to || c.UserName == to)).FirstOrDefault());
            var myContactPhoneNumber = myContact == null ? to : myContact.PhoneNumber;

            var sender = await _userService.GetUserByPhone(from);
            var receiver = await Task.Run(() => _applicationUser.FindByNameAsync(myContact == null ? to : myContact.PhoneNumber));
            var messages = (await Task.Run(() => _messageCollection.AsQueryable().Where(message =>
            ((message.SenderPhoneNumber == from) && (message.ReceiverPhoneNumber == receiver!.UserName)) ||
            ((message.SenderPhoneNumber == receiver!.UserName) && (message.ReceiverPhoneNumber == from))
            ))).ToList();

            Chat chat = new();
            foreach (var message in messages)
            {
                if (message.Status != null)
                {
                    message.Status = status;
                    message.TimeDelivered = DateTime.Now;
                    await _messageCollection.UpdateAsync(message.Id, message);
                    chat.Context = message.Context;
                    chat.Date = message.DateCreated.ToString();
                    chat.Status = status;
                    chat.ReceivedBy = myContact != null ? myContact!.UserName : to;
                    chat.SentBy = from;
                    chat.Type = MessageTypesEnums.Received.ToString();
                    chat.Time = message.TimeSent.Hour + ":" + message.TimeSent.Minute;
                }
            }
            await Clients.Client(connectionId: sender!.SignalRConnectionId).SendAsync("UpdateSentChat", chat);
        }
    }
}