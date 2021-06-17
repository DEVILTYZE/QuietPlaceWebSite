﻿namespace QuietPlaceWebProject.Interfaces
{
    // Пасскоды генерируются автоматически.
    // Пасскод определяет роль пользователя.
    // Пасскод разрешает отправлять сообщения с бОльшим количеством медиафайлов.
    // Пасскод отключает капчу.
    public interface IPasscode
    {
        int Id { get; set; }
        
        string Code { get; set; }
        
        IRole Role { get; set; }
    }
}