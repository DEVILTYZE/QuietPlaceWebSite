using System;

namespace QuietPlaceWebProject.Interfaces
{
    // Все права на взаимодействие с сообщениями есть у всех пользователей, исключая забаненных.
    public interface IPost
    {
        int Id { get; init; }
        
        string Text { get; set; }
        
        DateTime DateOfCreation { get; set; }
        
        int PosterId { get; set; }

        bool IsOriginalPoster { get; set; }
        
        int ThreadId { get; set; }

        // IEnumerable<int> ReworkTextPost();
        //
        // IEnumerable<Match> GetAnswerIdsStrings();
        //
        // IEnumerable<Match> GetGreenTextStrings();
    }
}