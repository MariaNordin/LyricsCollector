﻿using LyricsCollector.Context;
using LyricsCollector.Entities;
using LyricsCollector.Models;
using LyricsCollector.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LyricsCollector.Services.ConcreteServices
{
    public class LyricsService : ILyricsService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly LyricsCollectorDbContext _context;

        LyricsResponseModel lyrics = new LyricsResponseModel();

        public LyricsService(IHttpClientFactory clientFactory, LyricsCollectorDbContext context, IMemoryCache memoryCache)
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _context = context;
        }

        public IEnumerable<Lyrics> GetDbLyrics()
        {

            if (!_memoryCache.TryGetValue("DbLyrics", out List<Lyrics> listOfLyrics))
            {
                _memoryCache.Set("DbLyrics", _context.Lyrics.ToList());
            }

            listOfLyrics = _memoryCache.Get("DbLyrics") as List<Lyrics>;

            return listOfLyrics;
        }

        public async Task<LyricsResponseModel> Search(string artist, string title)
        {
            if (_memoryCache.TryGetValue("Lyrics", out LyricsResponseModel lyricsResponse))
                return (lyricsResponse);
            else
            {
                lyrics.Artist = ToTitleCase(artist);
                lyrics.Title = ToTitleCase(title);

                var existingLyrics = CheckIfLyricsInDb(lyrics.Artist, lyrics.Title);

                if (existingLyrics != null)
                {
                    lyrics.Lyrics = existingLyrics.SongLyrics;

                    _memoryCache.Set("Lyrics", lyrics);
                    return lyrics;
                }
                else
                {
                    var client = _clientFactory.CreateClient("lyrics");

                    try
                    {
                        lyrics = await client.GetFromJsonAsync<LyricsResponseModel>($"{artist}/{title}");

                        if (lyrics.Lyrics != "")
                        {
                            lyrics.Artist = ToTitleCase(artist);
                            lyrics.Title = ToTitleCase(title);
                            await SaveLyrics(lyrics);
                        }
                        _memoryCache.Set("Lyrics", lyrics);
                        return lyrics;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            
        }

        private async Task SaveLyrics(LyricsResponseModel lyricsRM)
        {
            var lyrics = new Lyrics
            {
                Artist = lyricsRM.Artist,
                Title = lyricsRM.Title,
                SongLyrics = lyricsRM.Lyrics
            };
            _context.Lyrics.Add(lyrics);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // log 
            }
        }

        private Lyrics CheckIfLyricsInDb(string artist, string title)
        {
            var lyricsInDb = GetDbLyrics();
            var existingLyrics = lyricsInDb.Where(l => l.Artist == artist && l.Title == title).FirstOrDefault();

            if (existingLyrics != null)
            {
                return existingLyrics;
            }

            return null;
        }

        //public Task<string> SaveToListAsync(Lyrics lyrics, int userId, int collectionId)
        //{
        //    var existingCollection = _context.Collections.Where(c => c.CollectionOfUserId == userId).FirstOrDefault();

        //    if (existingCollectionLyrics != null)
        //    {

        //    }
        //    var existingCollectionLyrics = _context.CollectionLyrics.Where(cl => cl.CollectionId == userRM.CollectionId).FirstOrDefault();

        //    //var existingLyrics = _context.Lyrics.Where(l => l.)
        //    //var isInList = CheckLyricsInExistingList(lyricsRM, userRM.CollectionId);

        //    var lyrics = new Lyrics
        //    {
        //        Artist = lyricsRM.Artist,
        //        Title = lyricsRM.Title,
        //        SongLyrics = lyricsRM.Lyrics
        //    };

        //    if (existingCollection != null)
        //    {
        //        existingCollection.Lyrics.Add(lyrics);
        //    }
        //    return "hej";

        //}

        //private bool CheckLyricsInExistingList(LyricsResponseModel lyricsRM, int collectionId)
        //{
        //    var existingLyrics = _context.Collections.Where(c => c.Id == collectionId)
        //}

        private static string ToTitleCase(string text)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(text);
        }
    }
}
