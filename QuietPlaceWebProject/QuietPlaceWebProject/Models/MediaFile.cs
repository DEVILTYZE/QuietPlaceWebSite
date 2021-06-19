using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Models
{
    public class MediaFile : IMediaFile
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int PostId { get; set; }
    }
}