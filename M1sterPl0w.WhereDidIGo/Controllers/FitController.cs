using Dynastream.Fit;
using Microsoft.AspNetCore.Mvc;

namespace M1sterPl0w.WhereDidIGo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FitController : ControllerBase
    {
        private readonly ILogger<FitController> _logger;

        public FitController(ILogger<FitController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "GetWeatherForecast")]
        public IActionResult UploadFile(List<IFormFile> files)
        {
            foreach (var file in files)
            {
                // Path to the FIT file
                var x = file.OpenReadStream();
                var fitFilePath = Path.GetTempFileName();

                // Save the file to the temporary location
                using (var fileStream = new FileStream(fitFilePath, FileMode.Create))
                {
                    x.CopyTo(fileStream);
                }

                // Open and decode the FIT file
                using (FileStream fitSource = new FileStream(fitFilePath, FileMode.Open))
                {
                    Decode decoder = new Decode();
                    if (decoder.IsFIT(fitSource) && decoder.CheckIntegrity(fitSource))
                    {
                        MesgBroadcaster mesgBroadcaster = new MesgBroadcaster();

                        // Create an event handler to capture GPS points
                        decoder.MesgEvent += mesgBroadcaster.OnMesg;
                        decoder.MesgDefinitionEvent += mesgBroadcaster.OnMesgDefinition;

                        // Decode the file
                        decoder.MesgEvent += mesgBroadcaster.OnMesg;

                        try
                        {
                            mesgBroadcaster.ActivityMes gEvent += OnActivityMesg;
                            mesgBroadcaster.RecordMesgEvent += OnRecordMesg;
                            mesgBroadcaster.SessionMesgEvent += OnSessionMesg;
                            decoder.Read(fitSource);


                        }
                        catch (FitException ex)
                        {
                            // Handle FIT Decode Exception
                        }
                        catch (System.Exception ex)
                        {
                            // Handle Exception
                        }
                    }
                    else
                    {
                        Console.WriteLine("FIT file is not valid or corrupted.");
                    }
                }
            }
            return Ok();
        }

        private void OnSessionMesg(object sender, MesgEventArgs e)
        {
            var x = (SessionMesg)e.mesg;
            var z = x.GetSport();

            var y = x.GetSportProfileName();
            // Get the activity type
            var activityType = x.GetFieldValue(ActivityMesg.FieldDefNum.Type) as string;
            Console.WriteLine($"NAME: {activityType}");
        }

        private void OnRecordMesg(object sender, MesgEventArgs e)
        {
            if (e.mesg.Name == "Record")
            {
                var x = (RecordMesg)e.mesg;

                Console.WriteLine($"LAT: '{x.GetPositionLat()}, LONG: '{x.GetPositionLong()}'");
            }
        }

        private void OnActivityMesg(object sender, MesgEventArgs e)
        {
            if (e.mesg.Name == "Activity")
            {
                var x = (ActivityMesg)e.mesg;
                // Get the activity type
                var activityType = x.GetFieldValue(ActivityMesg.FieldDefNum.Type) as string;
                Console.WriteLine($"NAME: {activityType}");
            }
        }
    }
}
