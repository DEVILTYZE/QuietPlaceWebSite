using System;

namespace QuietPlaceWebProject.Interfaces
{
    public enum MarkOfMessage
    {
        Anon,
        OriginalPoster
    }
    
    // Все права на взаимодействие с сообщениями есть у всех пользователей, исключая забаненных.
    public interface IPost
    {
        int Id { get; set; }
        
        string Text { get; set; }
        
        DateTime DateOfCreation { get; set; }
        
        IUser Sender { get; set; }
        
        IPost PostOfTovarishch { get; set; }

        MarkOfMessage Mark { get; set; }
        
        IThread Thread { get; set; }

        bool IsOriginalPoster();
    }
}