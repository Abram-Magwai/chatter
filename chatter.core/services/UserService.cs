using System.Security.Claims;
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
        public async Task<bool> CreateUserAsync(string phone, string Password)
        {
            if (!string.IsNullOrWhiteSpace(phone) && !string.IsNullOrWhiteSpace(Password))
            {
                ApplicationUser applicationUser = new() { 
                    UserName = phone, 
                    PhoneNumber = phone 
                };
                string error;
                try
                {
                    IdentityResult identityResult = await _applicationUser.CreateAsync(applicationUser, Password);
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
            ApplicationUser currentUser = await _applicationUser.FindByNameAsync(user.Username);
            var result = await _applicationUser.CheckPasswordAsync(currentUser, user.Password);
            if(result)
                return currentUser;
            return new();           
        }
        public async Task<ApplicationUser> GetUserByPhone(string phone)
        {
            ApplicationUser currentUser = await _applicationUser.FindByNameAsync(phone);
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
                claims.Add(new Claim(type: ClaimTypes.MobilePhone, appUser.Result.UserName));

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);
                return claimsIdentity;
            }
            return new();
        }
    }
}