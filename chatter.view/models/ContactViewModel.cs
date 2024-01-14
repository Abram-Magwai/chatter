using System.ComponentModel.DataAnnotations;

namespace chatter.view.models
{
    public class ContactViewModel
    {
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = null!;
    }
}
