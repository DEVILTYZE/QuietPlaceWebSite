namespace QuietPlaceWebProject.Interfaces
{
    // Пасскоды генерируются автоматически.
    // Пасскод определяет роль пользователя.
    // Пасскод разрешает отправлять сообщения с бОльшим количеством медиафайлов.
    // Пасскод отключает капчу.
    public interface IPasscode
    {
        int Id { get; init; }
        
        string Code { get; set; }
        
        int RoleId { get; set; }
    }
}