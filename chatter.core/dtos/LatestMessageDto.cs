using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatter.core.dtos
{
    public class LatestMessageDto : IComparable<LatestMessageDto>
    {
        public string ContactUserName {get;set;} = null!;
        public string Context {get;set;} = null!;
        public string? Status {get;set;}
        public int UnreadMessages {get;set;} = 0;
        public string Time {get;set;} = null!;

        public int CompareTo(LatestMessageDto? latestMessageDto)
        {
           if(DateTime.Parse(this.Time) == DateTime.Parse(latestMessageDto!.Time)) return 0;
            return DateTime.Parse(this.Time) < DateTime.Parse(latestMessageDto.Time) ? 1 : -1;
        }
    }
}