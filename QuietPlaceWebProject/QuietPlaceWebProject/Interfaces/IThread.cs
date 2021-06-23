namespace QuietPlaceWebProject.Interfaces
{
    // Все права на взаимодействие с тредами есть у админа.
    // Право на удаление треда есть дополнительно у модераторов.
    // Право на создание треда есть дополнительно у всех пользователей, за исключением забаненных.
    public interface IThread
    {
        int Id { get; init; }
        
        string Name { get; set; }
        
        bool HasBumpLimit { get; set; }
        
        int BoardId { get; set; }
        
        int PosterId { get; set; }
    }
}