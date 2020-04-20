using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using ACNHItemTextureExporter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ACNHResourceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TextureController : ControllerBase
    {

        private readonly ILogger<TextureController> _logger;

        public TextureController(ILogger<TextureController> logger)
        {
            _logger = logger;
        }

        [HttpGet("extract/{modelName}")]
        public IActionResult GetTexture(string modelName)
        {
            modelName = Path.GetFileNameWithoutExtension(modelName);
            modelName = TextureLoader.GetCleanFilenameWithoutExtensions(modelName);
            var path = Path.Combine(@"C:\Users\wchill.CHILLY\Desktop\output\romfs\Model", $"{modelName}.Nin_NX_NVN.zs");

            if (!System.IO.File.Exists(path))
            {
                return NotFound($"{modelName}.Nin_NX_NVN.zs not found in romfs");
            }

            var texture = TextureLoader.DecompressAndLoadFile(path);
            if (texture == null)
            {
                return BadRequest($"{modelName}.Nin_NX_NVN.zs is not a valid target");
            }

            MemoryStream ms = new MemoryStream();
            BitmapExporter.SaveBitmap(texture, ms);
            ms.Position = 0;
            return File(ms, "image/png", $"{texture.Name}.png");
        }

        [HttpGet]
        public IEnumerable<string> GetFilenames()
        {
            return Directory.EnumerateFiles(@"C:\Users\wchill.CHILLY\Desktop\output\romfs\Model")
                .Select(TextureLoader.GetCleanFilenameWithoutExtensions);
        }
    }
}
