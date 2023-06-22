using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.dtos;
using chatter.core.entities;
using chatter.core.models;

namespace chatter.core.interfaces
{
    public interface IProfileService
    {
        Task AddContactAsync(Person contact, string creatorPhoneNumber);
        Task<List<Contact>> GetContactsAsync(string userPhoneNumber);
        Task<ContactDto> GetContactAsync(string id);
        Task<string> GetContactPhoneNumberAsync(string userPhoneNumber, string userName);
    }
}