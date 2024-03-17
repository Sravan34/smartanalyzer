using SmartAnalyzer.Models;

namespace SmartAnalyzer.Services
{
    public interface IBlobContainerService
    {
        Task<List<MatchResponse>> GetAllBlobsByContainer(SearchRequest searchRequest);
    }
}
