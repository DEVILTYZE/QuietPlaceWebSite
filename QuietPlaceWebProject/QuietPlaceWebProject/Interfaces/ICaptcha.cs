namespace QuietPlaceWebProject.Interfaces
{
    public interface ICaptcha
    {
        int Id { get; set; }
        
        string ImageUrl { get; set; }
        
        string Word { get; set; }
    }
}