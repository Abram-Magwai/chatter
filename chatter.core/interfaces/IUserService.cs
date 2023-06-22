using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using chatter.core.entities;
using chatter.core.models;
using Microsoft.AspNetCore.Http;

namespace chatter.core.interfaces
{
    public interface IUserService
    {
        Task CreateUserAsync(string username, string password);
        Task<ClaimsIdentity> AuthenticateAsync(User user, HttpContext httpContext);
        Task<ApplicationUser> GetUser(User user);
        Task<ApplicationUser> GetUserByPhone(string phone);
    }
}