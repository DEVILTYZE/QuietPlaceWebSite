using System;

namespace QuietPlaceWebProject.Interfaces
{
    // Все права на взаимодействие с сообщениями есть у всех пользователей, исключая забаненных.
    public interface IPost
    {
        int Id { get; set; }
        
        string Text { get; set; }
        
        DateTime DateOfCreation { get; set; }
        
        int SenderId { get; set; }
        
        int PostOfTovarishchId { get; set; }

        bool IsOriginalPoster { get; set; }
        
        int ThreadId { get; set; }
    }
}