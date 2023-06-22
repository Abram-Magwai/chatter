using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using chatter.core.entities;
using chatter.core.interfaces;
using chatter.core.models;
using chatter.view.models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace chatter.view.Pages.account
{
    public class Login : PageModel
    {
        private readonly ILogger<Login> _logger;
        private readonly IUserService _userService;
        [BindProperty]
        public LoginCredentials Logins { get; set; } = null!;
        private readonly ISignalRHub _signalRHub;
        public Login(ILogger<Login> logger, IUserService userService, ISignalRHub signalRHub)
        {
            _logger = logger;
            _userService = userService;
            _signalRHub = signalRHub;
        }

        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var claimsIdentity = _userService.AuthenticateAsync(new User { Username = Logins.PhoneNumber, Password = Logins.Password }, HttpContext).Result;
                if (claimsIdentity.IsAuthenticated)
                {
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties() { IsPersistent = true }
                    );
                    return RedirectToPage("/Index");
                }
            }
            return Page();
        }
    }
}