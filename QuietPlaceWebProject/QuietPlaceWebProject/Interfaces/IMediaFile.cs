namespace QuietPlaceWebProject.Interfaces
{
    public interface IMediaFile
    {
        int Id { get; init; }
        
        string Url { get; set; }
        
        string Type { get; set; }
        
        int PostId { get; set; }
        
        int ThreadId { get; set; }
    }
}