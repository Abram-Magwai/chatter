using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.dtos;
using chatter.core.entities;
using chatter.core.interfaces;
using chatter.core.models;
using Microsoft.AspNetCore.Identity;

namespace chatter.core.services
{
    public class ProfileService : IProfileService
    {
        private readonly IMongoRepository<Contact> _contactCollection;
        private readonly IMongoRepository<Message> _messageCollection;
        UserManager<ApplicationUser> _applicationUser;
        public ProfileService(IMongoRepository<Contact> contactCollection, UserManager<ApplicationUser> applicationUser, IMongoRepository<Message> messageCollection)
        {
            _contactCollection = contactCollection;
            _applicationUser = applicationUser;
            _messageCollection = messageCollection;
        }
        public async Task AddContactAsync(Person person, string creatorPhoneNumber)
        {
            var contact = new Contact
            {
                CreatorPhoneNumber = creatorPhoneNumber,
                UserName = person.UserName,
                PhoneNumber = person.PhoneNumber
            };
            await _contactCollection.CreateAsync(contact);
        }
        public async Task<List<Contact>> GetContactsAsync(string userPhoneNumber)
        {
            var contacts = (await Task.Run( () => _contactCollection.AsQueryable().Where(c => c.CreatorPhoneNumber == userPhoneNumber))).ToList();
            return contacts;
        }
        public async Task<string> GetContactPhoneNumberAsync(string userPhoneNumber, string userName)
        {
            var contact = (await Task.Run( () => _contactCollection.AsQueryable().Where(c => c.CreatorPhoneNumber == userPhoneNumber && c.UserName == userName))).FirstOrDefault();
            string contactPhoneNumber = string.Empty;
            if(contact != null)
                return contact.PhoneNumber;
            return userName;
        }
        public async Task<ContactDto> GetContactAsync(string id)
        {
            var contact = await _contactCollection.GetAsync(id);
            if (contact == null)
                throw new Exception("Contact does not exist");
            var contactDto = new ContactDto
            {
                ContactId = contact.Id,
                UserName = contact.UserName,
                PhoneNumber = contact.PhoneNumber,
            };
            contactDto.IsMember = await _applicationUser.FindByNameAsync(contact.UserName) == null ? false : true;
            return contactDto;
        }
    }
}