using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MyCloud.Server.Api.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _logger;
        private readonly ServerOptions _serverOptions;

        public StorageController(ILogger<StorageController> logger, IOptions<ServerOptions> serverOptions)
        {
            _logger = logger;
            _serverOptions = serverOptions.Value;
        }

        [HttpPost("[action]")]
        public async Task<FileResult> DownloadFile([FromBody] string filePath)
        {
            var file = Path.Combine(_serverOptions.StorageDirectory, filePath);
            var fileStream = await System.IO.File.ReadAllBytesAsync(file);
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(file, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(fileStream, contentType, Path.GetFileName(file));
        }
    }
}
