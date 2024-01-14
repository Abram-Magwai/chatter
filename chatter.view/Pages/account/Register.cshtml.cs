using AutoMapper;
using chatter.core.interfaces;
using chatter.core.models;
using chatter.view.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace chatter.view.Pages.account
{
    public class Register : PageModel
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        [BindProperty]
        public RegistrationViewModel UserModel {get;set;} = null!;
        public Register(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPost() {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            User user = _mapper.Map<User>(UserModel);
            bool createdSuccessfully = await _userService.CreateUserAsync(user);
            if(!createdSuccessfully)
            {
                ViewData["errorMessage"] = "Something went wrong registering, try later";
                return Page();
            }
            return RedirectToPage("/Account/SucessRegistrationConfirmation");
        }
    }
}