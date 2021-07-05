using System;
using System.Collections.Generic;
using QuietPlaceWebProject.Models;

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
        
        string MediaUrl { get; set; }

        // IEnumerable<int> ReworkTextPost();
        //
        // IEnumerable<Match> GetAnswerIdsStrings();
        //
        // IEnumerable<Match> GetGreenTextStrings();
    }
}