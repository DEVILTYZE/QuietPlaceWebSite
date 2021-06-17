namespace QuietPlaceWebProject.Interfaces
{
    public interface IMediaFile
    {
        int Id { get; set; }
        
        string Path { get; set; }
        
        IPost Post { get; set; }
    }
}