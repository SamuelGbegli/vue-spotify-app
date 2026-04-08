using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using vue_spotify_app.Classes;

namespace vue_spotify_app.Server
{
    public class SpotifyAPIWrapper
    {
        private readonly AuthService _authService;
        private readonly HttpClient _httpClient;

        public SpotifyAPIWrapper(AuthService authService, HttpClient httpClient)
        {
            _authService = authService;
            _httpClient = httpClient;
        }

        async Task<HttpResponseMessage> SendGetMessage(string endpoint, string? token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/{endpoint}");
            if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request);
        }

        async Task<HttpResponseMessage> SendPostMessage(string endpoint, string? token, object? body)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.spotify.com/v1/{endpoint}")
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _httpClient.SendAsync(request);
        }

        public async Task<T> GetAsync<T>(Guid userId, string endpoint)
        {
            var token = await _authService.GetValidToken(userId);

            var response = await SendGetMessage(endpoint, token);

            while(!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _authService.GetValidToken(userId);
                    response = await SendGetMessage(endpoint, token);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
                    await Task.Delay(retryAfter);
                    response = await SendGetMessage(endpoint, token);
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }   

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>()!;
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/{endpoint}");

            var response = await SendGetMessage(endpoint, null);

            while (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
                    await Task.Delay(retryAfter);
                    response = await SendGetMessage(endpoint, null);
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>()!;
        }

        public async Task<T> GetAsync<T>(string endpoint, SpotifyToken token)
        {

            var response = await SendGetMessage(endpoint, token.AccessToken);

            while (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
                    await Task.Delay(retryAfter);
                    response = await SendGetMessage(endpoint, token.AccessToken);
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>()!;
        }

        public async Task<T> PostAsync<T>(Guid userId, string endpoint, object? body)
        {
            var token = await _authService.GetValidToken(userId);
            var response = await SendPostMessage(endpoint, token, body);
            while (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _authService.GetValidToken(userId);
                    response = await SendPostMessage(endpoint, token, body);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
                    await Task.Delay(retryAfter);
                    response = await SendPostMessage(endpoint, token, body);
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>()!;
        }

        public async Task PostAsync(Guid userId, string endpoint, object? body)
        {
            var token = await _authService.GetValidToken(userId);

            var response = await SendPostMessage(endpoint, token, body);
           while (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await _authService.GetValidToken(userId);
                    response = await SendPostMessage(endpoint, token, body);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
                    await Task.Delay(retryAfter);
                    response = await SendPostMessage(endpoint, token, body);
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            response.EnsureSuccessStatusCode();
        }
    }
}
