using System.ComponentModel.DataAnnotations;

namespace chatter.view.models
{
    public class RegistrationViewModel
    {
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}