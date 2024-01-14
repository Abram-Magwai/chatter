using System.Security.Claims;
using AutoMapper;
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
        private readonly IMapper _mapper;
        [BindProperty]
        public LoginViewModel LoginDetails { get; set; } = null!;
        public Login(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                User user = _mapper.Map<User>(LoginDetails);
                var claimsIdentity = _userService.AuthenticateAsync(user, HttpContext).Result;
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