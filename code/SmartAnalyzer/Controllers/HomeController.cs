using Microsoft.AspNetCore.Mvc;
using SmartAnalyzer.Models;
using SmartAnalyzer.Services;
using System.Diagnostics;

namespace SmartAnalyzer.Controllers
{
    
    public class HomeController : Controller
    {
        List<MatchResponse> matchResponse = new List<MatchResponse>();

        private readonly ILogger<HomeController> _logger;
        private readonly IBlobContainerService _blobContainerService;
        public HomeController(ILogger<HomeController> logger, IBlobContainerService blobContainerService)
        {
            _logger = logger;
            _blobContainerService = blobContainerService;
        }
        
        public async Task<IActionResult> Index(string context, string category, string noOfMatches, string threshold, string inputPath)
        {
            if (context != null)
            {
                SearchRequest searchRequest = new SearchRequest() { context = context , category = category, noOfMatches = int.Parse(noOfMatches) , threshold = decimal.Parse(threshold),inputFilePath = inputPath };

                matchResponse = await _blobContainerService.GetAllBlobsByContainer(searchRequest);
                BestMatch bm = new BestMatch();
                bm.bestMatchResponse = new List<BestMatchResponse>();

                foreach (var item in matchResponse)
                {
                    string id = "";
                    string filePath = "";
                    double score = 0;
                    foreach (var result in item.results)
                    {
                        id = result.id;
                        filePath = result.path;
                        score = result.score;
                    }

                    bm.bestMatchResponse.Add(new BestMatchResponse() 
                    { count = item.count, metaDataconfidenceScore = item.metadata.confidenceScore , status = item.status, id = id, path = filePath,score=score});
                }

                if (matchResponse.Count > 0)
                {

                }
            }
            return RedirectToAction("Index", "Home"); 
        }      
    }
}
