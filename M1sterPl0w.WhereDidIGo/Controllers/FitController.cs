using M1sterPl0w.WhereDidIGo.Services;
using Microsoft.AspNetCore.Mvc;

namespace M1sterPl0w.WhereDidIGo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FitController(ILogger<FitController> logger, IFitService fitService) : ControllerBase
    {
        private readonly ILogger<FitController> _logger = logger;
        private readonly IFitService _fitService = fitService;

        [HttpPost]
        [Route("[action]")]
        public IActionResult UploadZipAsync(IFormFile file)
        {
            var result = _fitService.DecodeZipAndWrapGeoBySport(file);
            var x = System.Text.Json.JsonSerializer.Serialize(result);

            return Ok(result);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult UploadFiles(List<IFormFile> files)
        {
            var result = _fitService.DecodeFilesAndWrapGeoBySport(files);
            return Ok(result);
        }
    }
}
