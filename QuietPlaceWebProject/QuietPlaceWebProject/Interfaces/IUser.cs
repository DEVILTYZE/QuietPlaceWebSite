using System.Collections.Generic;
using System.Net;

namespace QuietPlaceWebProject.Interfaces
{
    public interface IUser
    {
        IPAddress AddressOfUser { get; set; }
        
        IRole Role { get; set; }
        
        IEnumerable<IMessage> Answers { get; set; }
        
        IEnumerable<IThread> FavoriteThreads { get; set; }
    }
}