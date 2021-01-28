﻿using LyricsCollector.Entities;
using LyricsCollector.Models.UserModels;
using System.Threading.Tasks;

namespace LyricsCollector.Services.Contracts
{
    public interface IUserService
    {
        User GeneratePassword(UserPostModel userPM);

        UserWithToken ValidatePassword(UserPostModel userPM, User user);
    }
}
