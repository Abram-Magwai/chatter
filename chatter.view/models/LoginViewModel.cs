using System.ComponentModel.DataAnnotations;

namespace chatter.view.models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
