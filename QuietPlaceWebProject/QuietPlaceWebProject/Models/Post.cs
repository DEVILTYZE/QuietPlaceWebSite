using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    public class Post : IPost
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string DateOfCreation { get; set; }
        public int SenderId { get; set; }
        public int PostOfTovarishchId { get; set; }
        public bool IsOriginalPoster { get; set; }
        public int ThreadId { get; set; }
    }
}