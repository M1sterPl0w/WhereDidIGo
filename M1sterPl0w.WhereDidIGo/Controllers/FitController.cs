using System.IO.Compression;
using Dynastream.Fit;
using M1sterPl0w.WhereDidIGo.Models;
using Microsoft.AspNetCore.Mvc;

namespace M1sterPl0w.WhereDidIGo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FitController(ILogger<FitController> logger) : ControllerBase
    {
        private readonly ILogger<FitController> _logger = logger;

        [HttpPost]
        [Route("[action]")]
        public IActionResult UploadZipAsync(IFormFile file)
        {
            using var zipArchive = new ZipArchive(file.OpenReadStream());

            var result = zipArchive.Entries.Where(x => !string.IsNullOrWhiteSpace(x.Name));

            foreach (ZipArchiveEntry entry in result)
            {
                using var entryStream = entry.Open();
                using MemoryStream reader = new();
                entryStream.CopyTo(reader);

                reader.Position = 0;
                ProcessFile(reader);
            }

            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult UploadFiles(List<IFormFile> files)
        {
            foreach (var file in files)
            {
                using Stream fitSource = file.OpenReadStream();
                ProcessFile(fitSource);
            }

            return Ok();
        }

        private void ProcessFile(Stream fitSource)
        {
            var decoder = new Decode();
            if (decoder.IsFIT(fitSource) && decoder.CheckIntegrity(fitSource))
            {
                var mesgBroadcaster = new MesgBroadcaster();

                decoder.MesgEvent += mesgBroadcaster.OnMesg;
                decoder.MesgDefinitionEvent += mesgBroadcaster.OnMesgDefinition;

                try
                {
                    mesgBroadcaster.RecordMesgEvent += OnRecordMesg;
                    mesgBroadcaster.SessionMesgEvent += OnSessionMesg;
                    decoder.Read(fitSource);
                }
                catch (FitException ex)
                {
                    // Handle FIT Decode Exception
                }
            }
            else
            {
                Console.WriteLine("FIT file is not valid or corrupted.");
            }
        }

        private void OnSessionMesg(object sender, MesgEventArgs e)
        {
            var x = (SessionMesg)e.mesg;
            var z = x.GetSport();

            Console.WriteLine($"SPORT: {z}");
        }

        private void OnRecordMesg(object sender, MesgEventArgs e)
        {
            if (e.mesg.Name == "Record")
            {
                var x = (RecordMesg)e.mesg;

                var coordinate = new GeoCoordinate { Latitude = x.GetPositionLat(), Longitude = x.GetPositionLong() };



            }
        }
    }
}
