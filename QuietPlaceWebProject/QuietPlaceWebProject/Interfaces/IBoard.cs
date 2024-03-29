﻿namespace QuietPlaceWebProject.Interfaces
{
    // Все права на взаимодействие с досками есть у админа.
    public interface IBoard
    {
        int Id { get; init; }
        
        string Name { get; set; }
        
        string DomainName { get; set; }
        
        string ImageUrl { get; set; }
        
        string Description { get; set; }
        
        int MaxCountOfThreads { get; set; }
        
        bool IsHidden { get; set; }
        
        int AccessRoleId { get; set; }
    }
}