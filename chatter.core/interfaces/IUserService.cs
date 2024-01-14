using System.Security.Claims;
using chatter.core.entities;
using chatter.core.models;
using Microsoft.AspNetCore.Http;

namespace chatter.core.interfaces
{
    public interface IUserService
    {
        Task<bool> CreateUserAsync(User user);
        Task<ClaimsIdentity> AuthenticateAsync(User user, HttpContext httpContext);
        Task<ApplicationUser> GetUser(User user);
        Task<ApplicationUser> GetUserByPhone(string phone);
        Task<ApplicationUser> GetUserByUserName(string userName);
    }
}