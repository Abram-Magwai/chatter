using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatter.core.models
{
    public class Chat : IComparable<Chat>
    {
        public string SentBy {get;set;} = null!;
        public string ReceivedBy {get;set;} = null!;
        public string Date {get;set;} = null!;
        public string Context {get;set;} = null!;
        public string Type {get;set;} = null!;
        public string Status {get;set;} = null!;
        public string Time {get;set;} = null!;

        public int CompareTo(Chat? other)
        {
            if(this.Date == other!.Date) {
               if(DateTime.Parse(this.Time) == DateTime.Parse(other.Time)) return 0;
               return DateTime.Parse(this.Time) < DateTime.Parse(other.Time) ? -1 : 1;
            }
            return this.Date == "Today" && other.Date == "Yesterday" ? -1 : 1;
        }
    }
}