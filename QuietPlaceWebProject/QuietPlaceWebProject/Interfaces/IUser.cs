﻿namespace QuietPlaceWebProject.Interfaces
{
    // Пользователь будет записываться (обновляться) в БД, когда напишет сообщение, создаст тред или введёт пасскод.
    // Пользователи, не имеющие пасскод, хранятся в БД 24 часа.
    // Пользователи, имеющие пасскод, хранятся в БД столько, сколько действует пасскод плюс 1 день.
    // Все права на взаимодействие с пользователями есть у админа.
    // Право на бан пользователя есть дополнительно у модераторов.
    public interface IUser
    {
        string AddressOfUser { get; set; }
        
        int PasscodeId { get; set; }
    }
}