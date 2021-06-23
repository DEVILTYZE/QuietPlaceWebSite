namespace QuietPlaceWebProject.Interfaces
{
    public interface IMediaFile
    {
        int Id { get; init; }
        
        string Path { get; set; }
        
        int PostId { get; set; }
    }
}