using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatter.core.settings
{
    public class AppSettings
    {
        public string SecretKey {get;set;} = null!;
        public string Issuer {get;set;} = null!;
        public string Audience {get;set;} = null!;
    }
}