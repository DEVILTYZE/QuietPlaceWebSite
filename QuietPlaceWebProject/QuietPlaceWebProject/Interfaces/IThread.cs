using System.Collections.Generic;

namespace QuietPlaceWebProject.Interfaces
{
    public interface IThread
    {
        int Id { get; set; }
        
        string Name { get; set; }
        
        int BumpLimit { get; set; }
        
        IEnumerable<IMessage> Messages { get; set; }
    }
}