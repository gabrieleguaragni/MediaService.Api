using FluentValidation;
using MediaService.Business.Abstractions.Services;
using MediaService.Shared.DTO.Response;
using Microsoft.AspNetCore.Mvc;

namespace MediaService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IImageService _imageService;
        private IValidator<IFormFile> _imageValidator;

        public ImageController(
            IAuthService authService,
            IImageService imageService,
            IValidator<IFormFile> imageValidator
            )
        {
            _authService = authService;
            _imageService = imageService;
            _imageValidator = imageValidator;
        }

        [HttpPost("upload/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var validationResult = await _imageValidator.ValidateAsync(file);
            if (!validationResult.IsValid)
            {
                return StatusCode(400, new { message = validationResult.Errors.First().ErrorMessage });
            }

            ValidateTokenResponse validateTokenResponse = await _authService.TokenValidation(HttpContext.Request.Headers.Authorization);
            string fileName = await _imageService.UploadAvatar(validateTokenResponse.Roles, validateTokenResponse.IDUser, file);
            return Ok(new { image = fileName });
        }

        [HttpPost("upload/post")]
        public async Task<IActionResult> UploadPost(IFormFile file)
        {
            var validationResult = await _imageValidator.ValidateAsync(file);
            if (!validationResult.IsValid)
            {
                return StatusCode(400, new { message = validationResult.Errors.First().ErrorMessage });
            }

            ValidateTokenResponse validateTokenResponse = await _authService.TokenValidation(HttpContext.Request.Headers.Authorization);
            string fileName = await _imageService.UploadPost(validateTokenResponse.Roles, validateTokenResponse.IDUser, file);
            return Ok(new { image = fileName });
        }


        [HttpPost("delete/{fileName}")]
        public async Task<IActionResult> DeleteImage([FromRoute] string fileName)
        {
            ValidateTokenResponse validateTokenResponse = await _authService.TokenValidation(HttpContext.Request.Headers.Authorization);
            _imageService.DeleteImage(validateTokenResponse.Roles, fileName);
            return Ok(new { message = "Image deleted" });
        }

        [HttpGet("{fileName}")]
        public IActionResult GetImage([FromRoute] string fileName)
        {
            if (fileName == "default" || fileName == "default.png")
            {
                return File(_imageService.GetDefaultAvatar(), "image/png");
            }

            string filePath = _imageService.GetImage(fileName);
            return PhysicalFile(filePath, "image/png");
        }

        [HttpGet]
        public async Task<IActionResult> GetImageList()
        {
            ValidateTokenResponse validateTokenResponse = await _authService.TokenValidation(HttpContext.Request.Headers.Authorization);
            string[] files = _imageService.GetImageList(validateTokenResponse.Roles);
            return Ok(new { images = files });
        }
    }
}