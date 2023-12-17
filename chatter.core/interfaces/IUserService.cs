using System.Security.Claims;
using chatter.core.entities;
using chatter.core.models;
using Microsoft.AspNetCore.Http;

namespace chatter.core.interfaces
{
    public interface IUserService
    {
        Task<bool> CreateUserAsync(string username, string password);
        Task<ClaimsIdentity> AuthenticateAsync(User user, HttpContext httpContext);
        Task<ApplicationUser> GetUser(User user);
        Task<ApplicationUser> GetUserByPhone(string phone);
    }
}