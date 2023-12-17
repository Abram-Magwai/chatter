using System.Security.Claims;
using chatter.core.interfaces;
using chatter.core.models;
using chatter.view.models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace chatter.view.Pages.account
{
    public class Login : PageModel
    {
        private readonly IUserService _userService;
        [BindProperty]
        public LoginCredentials Logins { get; set; } = null!;
        public Login(IUserService userService)
        {
            _userService = userService;
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
                }else
                {
                    ViewData["errorMessage"] = "Incorrect Credentials";
                    return Page();
                }
            }
            return Page();
        }
    }
}