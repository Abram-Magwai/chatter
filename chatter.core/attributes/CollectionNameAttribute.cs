using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatter.core.attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionNameAttribute : Attribute
    {
        public string Name {get;}
        public CollectionNameAttribute(string name)
        {
            Name = name;
        }       
    }
}