namespace QuietPlaceWebProject.Interfaces
{
    // Все права на взаимодействие с досками есть у админа.
    public interface IBoard
    {
        int Id { get; set; }
        
        string Name { get; set; }
        
        string DomainName { get; set; }
        
        int MaxCountOfThreads { get; set; }
    }
}