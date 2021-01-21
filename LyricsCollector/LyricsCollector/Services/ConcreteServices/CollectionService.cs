﻿using LyricsCollector.Context;
using LyricsCollector.Entities;
using LyricsCollector.Events;
using LyricsCollector.Models.UserModels;
using LyricsCollector.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LyricsCollector.Services.ConcreteServices
{
    public class CollectionService : ICollectionService
    {
        private readonly LyricsCollectorDbContext _context;
        private User _loggedInUser;
       
        public CollectionService(LyricsCollectorDbContext context)
        {
            _context = context;
        }

        public void OnUserLoggedIn(object source, UserEventArgs args)
        {
            _loggedInUser = args.User;
        }

        public void OnRegisteredUser(object source, UserEventArgs args)
        {

        }
    }
}
