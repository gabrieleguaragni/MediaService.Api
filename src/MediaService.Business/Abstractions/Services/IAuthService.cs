using MediaService.Shared.DTO.Response;

namespace MediaService.Business.Abstractions.Services
{
    public interface IAuthService
    {
        public Task<ValidateTokenResponse> TokenValidation(string? token);
    }
}