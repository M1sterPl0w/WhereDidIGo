using M1sterPl0w.WhereDidIGo.Models;

namespace M1sterPl0w.WhereDidIGo.Services
{
    public interface IFitService
    {
        Dictionary<string, List<GeoCoordinate>> DecodeZipAndWrapGeoBySport(IFormFile zipFile);

        Dictionary<string, List<GeoCoordinate>> DecodeFilesAndWrapGeoBySport(List<IFormFile> files);
    }
}
