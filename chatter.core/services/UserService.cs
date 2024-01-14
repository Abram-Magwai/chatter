using System.Security.Claims;
using AspNetCore.Identity.MongoDbCore.Models;
using chatter.core.entities;
using chatter.core.interfaces;
using chatter.core.models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace chatter.core.services
{
    public class UserService : IUserService
    {
        UserManager<ApplicationUser> _applicationUser;

        public UserService(UserManager<ApplicationUser> applicationUser)
        {
            _applicationUser = applicationUser;
        }
        public async Task<bool> CreateUserAsync(User user) {
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber) && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                ApplicationUser applicationUser = new() { 
                    UserName = user.UserName, 
                    PhoneNumber = user.PhoneNumber,
                    Email = user.PhoneNumber
                };
                string error;
                try
                {
                    IdentityResult identityResult = await _applicationUser.CreateAsync(applicationUser, user.Password);
                    if (identityResult.Succeeded) return true;
                    else return false;
                }
                catch (Exception e)
                {                    
                    error = e.ToString();
                }                
            }
            return false;
        }
        public async Task<ApplicationUser> GetUser(User user)
        {
            ApplicationUser currentUser = await _applicationUser.FindByEmailAsync(user.PhoneNumber);
            var result = await _applicationUser.CheckPasswordAsync(currentUser, user.Password);
            if(result)
                return currentUser;
            return new();
        }
        public async Task<ApplicationUser> GetUserByPhone(string phone)
        {
            ApplicationUser currentUser = await _applicationUser.FindByEmailAsync(phone);
            return currentUser == null ? throw new Exception("User doesn't exists") : currentUser;
        }
        public async Task<ApplicationUser> GetUserByUserName(string userName)
        {
            ApplicationUser currentUser = await _applicationUser.FindByNameAsync(userName);
            return currentUser == null ? throw new Exception("User doesn't exists") : currentUser;
        }
        public async Task<ClaimsIdentity> AuthenticateAsync(User user, HttpContext httpContext)
        {
            var appUser = GetUser(user);
            if (appUser.Result.UserName != null)
            {
                appUser.Result.IsOnline = true;
                await _applicationUser.UpdateAsync(appUser.Result);
                List<Claim> claims = new();
                claims.Add(new Claim(type: ClaimTypes.MobilePhone, appUser.Result.PhoneNumber));
                claims.Add(new Claim(type: ClaimTypes.Name, appUser.Result.UserName));

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);
                return claimsIdentity;
            }
            return new();
        }
    }
}