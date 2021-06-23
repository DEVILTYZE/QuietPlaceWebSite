namespace QuietPlaceWebProject.Interfaces
{
    // Все права на взаимодействие с ролями есть у админа.
    public interface IRole
    {
        int Id { get; init; }
        
        string Name { get; set; }
    }
}