using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
        SignInManager<ApplicationUser> _signInManager;

        public UserService(UserManager<ApplicationUser> applicationUser, SignInManager<ApplicationUser> signInManager)
        {
            _applicationUser = applicationUser;
            _signInManager = signInManager;
        }
        public async Task CreateUserAsync(string Email, string Password)
        {
            if (!string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password))
            {
                ApplicationUser applicationUser = new ApplicationUser { UserName = Email, Email = Email };
                // _userRepository.InsertOne(applicationUser);
                string error;
                try
                {
                    IdentityResult identityResult = await _applicationUser.CreateAsync(
                    applicationUser,
                    Password
                );
                }
                catch (Exception e)
                {
                    
                    error = e.ToString();
                }                
            }
            else
                throw new Exception("Invalid credentials");
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
            if(currentUser == null) throw new Exception("User doesn't exists");
            return currentUser;       
        }
        public async Task<ClaimsIdentity> AuthenticateAsync(User user, HttpContext httpContext)
        {
            var appUser = GetUser(user);
            if (appUser.Result.UserName != null)
            {
                appUser.Result.IsOnline = true;
                await _applicationUser.UpdateAsync(appUser.Result);
                var claimsList = appUser.Result.Claims;
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