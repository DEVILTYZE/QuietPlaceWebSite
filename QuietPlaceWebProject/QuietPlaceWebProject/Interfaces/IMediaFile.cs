namespace QuietPlaceWebProject.Interfaces
{
    public interface IMediaFile
    {
        int Id { get; set; }
        
        string Path { get; set; }
        
        int PostId { get; set; }
    }
}