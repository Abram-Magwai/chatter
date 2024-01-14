using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatter.core.interfaces
{
    public interface IBlobService
    {
        Task AddBlob(string filePath, string fileName, Stream stream);
        Task DeleteBlob(string name);
        string GetBlob(string fileName);
    }
}
