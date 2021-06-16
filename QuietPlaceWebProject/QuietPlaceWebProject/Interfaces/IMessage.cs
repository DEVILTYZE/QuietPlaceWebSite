using System.Collections.Generic;

namespace QuietPlaceWebProject.Interfaces
{
    public enum MarkOfMessage
    {
        Anon,
        OriginalPoster
    }
    
    public interface IMessage
    {
        int Id { get; set; }
        
        string Text { get; set; }
        
        IUser Sender { get; set; }
        
        IMessage MessageOfTovarishch { get; set; }
        
        IEnumerable<IMessage> Answers { get; set; }

        MarkOfMessage Mark { get; set; }

        bool IsOriginalPoster();
    }
}