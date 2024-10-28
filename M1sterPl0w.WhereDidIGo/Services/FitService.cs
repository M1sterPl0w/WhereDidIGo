using M1sterPl0w.WhereDidIGo.Models;

namespace M1sterPl0w.WhereDidIGo.Services
{

    public interface IFitService
    {
        Dictionary<string, List<GeoCoordinate>> DecodeZipAndWrapGeoBySport(IFormFile zipFile);

        Dictionary<string, List<GeoCoordinate>> DecodeFilesAndWrapGeoBySport(List<IFormFile> files);
    }

    public sealed class FitService : IFitService
    {
        public Dictionary<string, List<GeoCoordinate>> DecodeFilesAndWrapGeoBySport(List<IFormFile> files)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, List<GeoCoordinate>> DecodeZipAndWrapGeoBySport(IFormFile zipFile)
        {
            throw new NotImplementedException();
        }
    }
}
