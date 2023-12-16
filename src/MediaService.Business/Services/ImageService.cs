using MediaService.Business.Abstractions.Services;
using MediaService.Business.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp.Formats.Png;
using System.Data;
using System.Text.RegularExpressions;

namespace MediaService.Business.Services
{
    public class ImageService : IImageService
    {
        private readonly string administrator = "administrator";
        private readonly string developer = "developer";
        private readonly IConfiguration _configuration;
        private readonly string _imagesPath;


        public ImageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _imagesPath = _configuration["ImagesPath"]!;
        }

        public async Task<string> UploadAvatar(List<string?> roles, long IDUser, IFormFile file)
        {
            roles = roles.Where(x => x != null).ToList();
            if (!roles.Contains(administrator) && !roles.Contains(developer))
            {
                throw new HttpStatusException(403, "You do not have permission to upload images directly");
            }

            if (!Regex.IsMatch(Path.GetExtension(file.FileName), "png|jpe?g", RegexOptions.IgnoreCase))
            {
                throw new HttpStatusException(415, "Invalid file extension");
            }

            string uniqueFileName = $"{IDUser}_{DateTime.Now.Ticks}_{Guid.NewGuid()}.png";
            string filePath = Path.Combine(_imagesPath, uniqueFileName);

            using MemoryStream memoryStream = new();
            file.OpenReadStream().CopyTo(memoryStream);
            await OptimizeAndSaveAvatar(memoryStream, filePath);

            return uniqueFileName;
        }

        public async Task UploadAvatar(string file, string fileExtension, string fileName)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
            {
                throw new HttpStatusException(415, "Invalid file extension");
            }

            string uniqueFileName = $"{fileName}.png";
            string filePath = Path.Combine(_imagesPath, uniqueFileName);

            using MemoryStream memoryStream = new(Convert.FromBase64String(file));
            await OptimizeAndSaveAvatar(memoryStream, filePath);
        }

        private async Task OptimizeAndSaveAvatar(MemoryStream memoryStream, string filePath)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            using Image image = await Image.LoadAsync(memoryStream);
            image.Mutate(x => x.Resize(120, 0, KnownResamplers.Lanczos3));
            image.Save(filePath, new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression,
            });
        }
        
        public void DeleteImage(List<string?> roles, string fileName)
        {
            roles = roles.Where(x => x != null).ToList();
            if (!roles.Contains(administrator) && !roles.Contains(developer))
            {
                throw new HttpStatusException(403, "You do not have permission to delete images");
            }

            string filePath = $"{_imagesPath}/{fileName}";
            if (!Regex.IsMatch(Path.GetExtension(filePath), "png", RegexOptions.IgnoreCase))
            {
                filePath += ".png";
            }

            if (!File.Exists(filePath))
            {
                throw new HttpStatusException(404, "Image not found");
            }

            File.Delete(filePath);
        }

        public string GetImage(string fileName)
        {
            string filePath = $"{_imagesPath}/{fileName}";
            if (!Regex.IsMatch(Path.GetExtension(filePath), "png", RegexOptions.IgnoreCase))
            {
                filePath += ".png";
            }

            if (!File.Exists(filePath))
            {
                throw new HttpStatusException(404, "Image not found");
            }

            return filePath;
        }

        public string[] GetImageList(List<string?> roles)
        {
            roles = roles.Where(x => x != null).ToList();
            if (!roles.Contains(administrator))
            {
                throw new HttpStatusException(403, "Invalid permission to access this resource");
            }

            string[] files = new DirectoryInfo(_imagesPath).GetFiles().Select(o => o.Name).ToArray();
            return files;
        }

        public byte[] GetDefaultAvatar()
        {
            return Convert.FromBase64String(_configuration["DefaultAvatar"]!);
        }

        public async Task<string> UploadPost(List<string?> roles, long IDUser, IFormFile file)
        {
            roles = roles.Where(x => x != null).ToList();
            if (!roles.Contains(administrator) && !roles.Contains(developer))
            {
                throw new HttpStatusException(403, "You do not have permission to upload images directly");
            }

            if (!Regex.IsMatch(Path.GetExtension(file.FileName), "png|jpe?g", RegexOptions.IgnoreCase))
            {
                throw new HttpStatusException(415, "Invalid file extension");
            }

            string uniqueFileName = $"{IDUser}_{DateTime.Now.Ticks}_{Guid.NewGuid()}.png";
            string filePath = Path.Combine(_imagesPath, uniqueFileName);

            using MemoryStream memoryStream = new();
            file.OpenReadStream().CopyTo(memoryStream);
            await OptimizeAndSavePost(memoryStream, filePath);

            return uniqueFileName;
        }

        public async Task UploadPost(string file, string fileExtension, string fileName)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
            {
                throw new HttpStatusException(415, "Invalid file extension");
            }

            string uniqueFileName = $"{fileName}.png";
            string filePath = Path.Combine(_imagesPath, uniqueFileName);

            using MemoryStream memoryStream = new(Convert.FromBase64String(file));
            await OptimizeAndSavePost(memoryStream, filePath);
        }

        private async Task OptimizeAndSavePost(MemoryStream memoryStream, string filePath)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            using Image image = await Image.LoadAsync(memoryStream);
            image.Mutate(x => x.Resize(300, 0, KnownResamplers.Lanczos3));
            image.Save(filePath, new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.DefaultCompression,
            });
        }
    }
}