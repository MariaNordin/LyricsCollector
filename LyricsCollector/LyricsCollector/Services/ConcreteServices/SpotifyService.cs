﻿using LyricsCollector.Models.SpotifyModels;
using LyricsCollector.Models.SpotifyModels.Contracts;
using LyricsCollector.Services.Contracts;
using LyricsCollector.SpotifyCredentials;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LyricsCollector.Services.ConcreteServices
{
    public class SpotifyService : ISpotifyService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly SpotifyCred _credentials;

        private ISpotifyTokenModel token;
        TrackResponseModel trackResponse;

        //private string currentToken;

        public SpotifyService(IHttpClientFactory clientFactory, IOptions<SpotifyCred> credentials)
        {
            _clientFactory = clientFactory;
            _credentials = credentials.Value;
        }

        public async Task<ITrackResponseModel> SearchAsync(string artist, string title)
        {
            if (token == null || token.IsExpired)
            {
                await GetAccessTokenAsync();
            }

            var queryString = HttpUtility.UrlEncode($"{artist} {title}");

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"search?q={queryString}&type=track&limit=1");
            request.Headers.Add("Authorization", $"Bearer {token.Access_token}");
            request.Headers.Add("Accept", "application/json");

            var client = _clientFactory.CreateClient("spotify");

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                trackResponse = await response.Content.ReadFromJsonAsync<TrackResponseModel>();
                return trackResponse;
            }
            else
            {
                return null;
            }
        }

        public async Task GetAccessTokenAsync()
        {
            var clientId = _credentials.SpotifyClientId;
            var clientSecret = _credentials.SpotifyClientSecret;
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                string.Format($"{clientId}:{clientSecret}")));

            var request = new HttpRequestMessage(HttpMethod.Post,
            "https://accounts.spotify.com/api/token");
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Headers.Add("Accept", "application/json");

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"grant_type", "client_credentials" }
            });

            var client = _clientFactory.CreateClient();

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                token = await response.Content.ReadFromJsonAsync<SpotifyTokenModel>();
            }
        }
    }
}
