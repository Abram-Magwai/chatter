using System.ComponentModel.DataAnnotations;

namespace chatter.core.models
{
    public class User
    {
        public string UserName {get;set;} = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Password {get;set;}
    }
}