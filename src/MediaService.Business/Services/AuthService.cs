using MediaService.Business.Abstractions.Services;
using MediaService.Business.Exceptions;
using MediaService.Shared.DTO.Response;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MediaService.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ValidateTokenResponse> TokenValidation(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new HttpStatusException(401, "Invalid token");
            }

            token = token.Replace("Bearer ", "");

            using var _httpClient = _httpClientFactory.CreateClient("AuthApiClient");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(_configuration["AuthApiClient:ValidateToken"]!);
            string content = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpStatusException((int)response.StatusCode, JsonSerializer.Deserialize<ErrorMessageResponse>(content, options));
            }
      
            ValidateTokenResponse? validateTokenResponse = JsonSerializer.Deserialize<ValidateTokenResponse>(content, options);
            return validateTokenResponse ?? throw new HttpStatusException((int)response.StatusCode, "Token validation error");
        }
    }
}