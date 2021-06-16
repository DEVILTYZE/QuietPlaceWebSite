using System.Collections.Generic;

namespace QuietPlaceWebProject.Interfaces
{
    public interface IBoard
    {
        int Id { get; set; }
        
        string Name { get; set; }
        
        int MaxCountOfThreads { get; set; }
        
        IEnumerable<IThread> Threads { get; set; }
    }
}