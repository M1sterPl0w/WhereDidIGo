using System.IO.Compression;
using Dynastream.Fit;
using M1sterPl0w.WhereDidIGo.Models;

namespace M1sterPl0w.WhereDidIGo.Services
{
    public sealed class FitService : IFitService
    {
        private readonly Dictionary<string, List<GeoCoordinate>> _geoBySport = [];
        private string _lastSport = string.Empty;
        private List<GeoCoordinate> _lastCoordinates = [];

        private int counter = 0;

        public Dictionary<string, List<GeoCoordinate>> DecodeFilesAndWrapGeoBySport(List<IFormFile> files)
        {
            foreach (var file in files)
            {
                using Stream fitSource = file.OpenReadStream();
                ProcessFile(fitSource);
            }

            return _geoBySport;
        }

        public Dictionary<string, List<GeoCoordinate>> DecodeZipAndWrapGeoBySport(IFormFile file)
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
            };

            return _geoBySport;
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
                    mesgBroadcaster.SessionMesgEvent += OnSessionMesg;
                    mesgBroadcaster.RecordMesgEvent += OnRecordMesg;
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
            var y = System.Text.Json.JsonSerializer.Serialize(_lastCoordinates);

            var x = (SessionMesg)e.mesg;
            var z = x.GetSport();

            if (!z.HasValue)
            {
                return; // TODO Maybe throw an exception?
            }

            if (!_geoBySport.ContainsKey(z.Value.ToString()))
            {
                _geoBySport.Add(z.Value.ToString(), []);
            }

            if (_lastCoordinates.Count != 0)
            {
                _geoBySport[z.Value.ToString()].AddRange(_lastCoordinates);
                _lastCoordinates.Clear();
            }
        }

        private void OnRecordMesg(object sender, MesgEventArgs e)
        {
            if (e.mesg.Name == "Record")
            {
                var x = (RecordMesg)e.mesg;

                _lastCoordinates.Add(new GeoCoordinate { Latitude = x.GetPositionLat(), Longitude = x.GetPositionLong() });

                counter++;
            }
        }
    }
}
