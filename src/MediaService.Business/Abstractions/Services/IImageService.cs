using Microsoft.AspNetCore.Http;

namespace MediaService.Business.Abstractions.Services
{
    public interface IImageService
    {
        public Task<string> UploadAvatar(List<string?> roles, long IDUser, IFormFile file);
        public Task UploadAvatar(string file, string fileExtension, string fileName);
        public Task<string> UploadPost(List<string?> roles, long IDUser, IFormFile file);
        public Task UploadPost(string file, string fileExtension, string fileName);
        public void DeleteImage(List<string?> roles, string fileName);
        public string GetImage(string fileName);
        public string[] GetImageList(List<string?> roles);
        public byte[] GetDefaultAvatar();
    }
}