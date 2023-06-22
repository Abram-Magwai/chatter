using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatter.core.interfaces;
using chatter.view.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace chatter.view.Pages.account
{
    public class Register : PageModel
    {
        private readonly ILogger<Register> _logger;
        private readonly IUserService _userService;
        [BindProperty]
        public LoginCredentials Logins {get;set;} = null!;
        public Register(ILogger<Register> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPost() {
            if(!ModelState.IsValid)
                return Page();
            await _userService.CreateUserAsync(Logins.PhoneNumber, Logins.Password);
            return RedirectToPage("/Account/Login");
        }
    }
}