using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using chatter.core.entities;
using chatter.core.interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace chatter.view.Pages.account
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;
        private readonly UserManager<ApplicationUser> _applicationUser;
        private readonly IUserService _userService;
        public LogoutModel(ILogger<LogoutModel> logger, UserManager<ApplicationUser> applicationUser, IUserService userService)
        {
            _logger = logger;
            _applicationUser = applicationUser;
            _userService = userService;
        }

        public Task<IActionResult> OnGet()
        {
            return OnPost();
        }
        public async Task<IActionResult> OnPost() {
            var use = HttpContext.User;
            var userPhoneNumber = HttpContext.User.Claims.AsQueryable().Where(x => x.Type == ClaimTypes.MobilePhone).First().Value;
            var user = await _userService.GetUserByPhone(userPhoneNumber);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            user.IsOnline = false;
            user.LastSeen = DateTime.Now;
            await _applicationUser.UpdateAsync(user);
            return RedirectToPage("/Index");
        }
    }
}