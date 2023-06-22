using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using chatter.core.dtos;
using chatter.core.entities;
using chatter.core.enums;
using chatter.core.interfaces;
using chatter.core.models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace chatter.view.Pages;
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IUserService _userService;
    private readonly IProfileService _profileService;
    private readonly UserManager<ApplicationUser> _applicationUser;
    private readonly IMessageService _messageService;
    [BindProperty]
    public Account AccountDetails {get;set;} = null!;
    [ViewData]
    public List<LatestMessageDto> RecentMessageDtos { get; set; }

    public IndexModel(
        ILogger<IndexModel> logger,
        IUserService userService,
        IProfileService profileService,
        IMessageService messageService,
        UserManager<ApplicationUser> applicationUser
    )
    {
        _logger = logger;
        _userService = userService;
        _profileService = profileService;
        _messageService = messageService;
        _applicationUser = applicationUser;
        RecentMessageDtos = new();
    }

    public async Task<IActionResult> OnGet()
    {
        var userPhoneNumber = HttpContext.User.Claims.AsQueryable().Where(x => x.Type == ClaimTypes.MobilePhone).First().Value;
        RecentMessageDtos = await _messageService.GetRecentMessages(userPhoneNumber);
        return Page();
    }
    public async Task<IActionResult> OnPost()
    {
        if(!ModelState.IsValid)
            return Page();
        var userPhoneNumber = HttpContext.User.Claims.AsQueryable().Where(x => x.Type == ClaimTypes.MobilePhone).First().Value;
        await _profileService.AddContactAsync(new Person
        {
            UserName = AccountDetails.UserName,
            PhoneNumber = AccountDetails.PhoneNumber
        }, userPhoneNumber);
        return Content(JsonSerializer.Serialize(new {outcome = "Success"}));
    }
    public async Task<IActionResult> OnGetData()
    {
        var userPhoneNumber = HttpContext.User.Claims.AsQueryable().Where(x => x.Type == ClaimTypes.MobilePhone).First().Value;
        var contacts = await _profileService.GetContactsAsync(userPhoneNumber);
        List<ContactDto> contactsDtos = new();
        for(int i = 0; i < contacts.Count; i++) {
            var contact =  contacts[i];
            var user = await _applicationUser.FindByNameAsync(contact.PhoneNumber);
            var contactDto = new ContactDto
            {
                ContactId = contact.Id,
                UserName = contact.UserName,
                PhoneNumber = contact.PhoneNumber,
                IsMember = user == null ? false : true
            };
            contactsDtos.Add(contactDto);
            if(i == contacts.Count-1) {
                var json = JsonSerializer.Serialize(contactsDtos);
                return Content(json);
            }
        }        
        return Content(JsonSerializer.Serialize(contactsDtos));
    }
    public async Task<IActionResult> OnGetConversation(string contactPhoneNumber)
    {
        var userPhoneNumber = HttpContext.User.Claims.AsQueryable().Where(x => x.Type == ClaimTypes.MobilePhone).First().Value;
        var conversation = await _messageService.GetConversationAsync(userPhoneNumber!, contactPhoneNumber);
        var json = JsonSerializer.Serialize(conversation);
        return Content(json);
    }
    public async Task<IActionResult> OnGetConversationByUsername(string contactUserName)
    {
        var userPhoneNumber = HttpContext.User.Claims.AsQueryable().Where(x => x.Type == ClaimTypes.MobilePhone).First().Value;
        var contactPhoneNumber = _profileService.GetContactPhoneNumberAsync(userPhoneNumber, contactUserName).Result;
        var conversation = await _messageService.GetConversationAsync(userPhoneNumber!, contactPhoneNumber);
        var json = JsonSerializer.Serialize(conversation);
        return Content(json);
    }
    public async Task<IActionResult> OnPostMessage(MessageDto messageDto, string connectionId)
    {
        var userPhoneNumber = HttpContext.User.Claims.AsQueryable().Where(x => x.Type == ClaimTypes.MobilePhone).First().Value;
        var receiverPhoneNumber = _profileService.GetContactPhoneNumberAsync(userPhoneNumber, messageDto.ReceiverPhoneNumber).Result;
        var message = new Message
        {
            SenderPhoneNumber = userPhoneNumber,
            ReceiverPhoneNumber = receiverPhoneNumber,
            TimeSent = DateTime.Now,
            Status = MessageStatusEnums.Sent.ToString(),
            Context = messageDto.Context
        };
        var newMessage = await _messageService.SendMessage(message);
        return Content(JsonSerializer.Serialize(newMessage));
    }
    public IActionResult OnGetPhoneNumber()
    {
        var phoneNumber = HttpContext.User.Claims.AsQueryable().Where(x => x.Type == ClaimTypes.MobilePhone).First().Value;
        return Content(phoneNumber);
    }
    public async Task OnPostUpdateConnectionId(string ConnectionId)
    {
        var phoneNumber = HttpContext.User.Claims.AsQueryable().Where(x => x.Type == ClaimTypes.MobilePhone).First().Value;
        var user = await _userService.GetUserByPhone(phoneNumber);
        user.SignalRConnectionId = ConnectionId;
        await _applicationUser.UpdateAsync(user);
    }
}
public class Account {
    [Required]
    [Display(Name = "Phone Number")]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber {get;set;} = null!;
    [Required]
    public string UserName {get;set;} = null!;
}
