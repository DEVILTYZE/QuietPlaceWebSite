namespace QuietPlaceWebProject.Interfaces
{
    // Все права на взаимодействие с ролями есть у админа.
    public interface IRole
    {
        int Id { get; set; }
        
        string Name { get; set; }
    }
}