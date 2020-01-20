using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using MyCloud.BL;

namespace MyCloud.Server.Api.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _logger;
        private readonly StorageService _storageService;

        public StorageController(ILogger<StorageController> logger, StorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        [HttpPost("[action]")]
        public async Task<FileResult> DownloadFile([FromBody] string filePath)
        {
            var file = await _storageService.GetFileAsync(filePath);
            return File(file, GetMimeType(filePath), Path.GetFileName(filePath));
        }

        [HttpPost("[action]")]
        public async Task UploadFile(IFormFile file, string filePath = "", bool overwrite = false)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file), "Отсутствует файл");
            }
            await _storageService.SaveFileToAsync(file.OpenReadStream(), Path.Combine(filePath, file.FileName), overwrite);
        }

        [HttpDelete("[action]")]
        public async Task DeleteFile([FromBody] string filePath)
        {
            await _storageService.DeleteFileAsync(filePath);
        }

        private string GetMimeType(string file)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(file, out var contentType))
            {
                contentType = MediaTypeNames.Application.Octet;
            }

            return contentType;
        }
    }
}
