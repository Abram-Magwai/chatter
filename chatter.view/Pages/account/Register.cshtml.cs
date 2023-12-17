using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.interfaces;
using chatter.view.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace chatter.view.Pages.account
{
    public class Register : PageModel
    {
        private readonly IUserService _userService;
        [BindProperty]
        public LoginCredentials Logins {get;set;} = null!;
        public Register(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPost() {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            bool createdSuccessfully = await _userService.CreateUserAsync(Logins.PhoneNumber, Logins.Password);
            if(!createdSuccessfully)
            {
                ViewData["errorMessage"] = "Something went wrong registering, try later";
                return Page();
            }
            return RedirectToPage("/Account/SucessRegistrationConfirmation");
        }
    }
}