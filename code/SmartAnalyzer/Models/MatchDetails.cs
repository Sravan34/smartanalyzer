using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartAnalyzer.Models
{
    public class BestMatch
    {
        public List<BestMatchResponse> bestMatchResponse { get; set; }
    }
    public class BestMatchResponse
    {
        public string status { get; set; }
        public int count { get; set; }
        public double metaDataconfidenceScore { get; set; }
        public string id { get; set; }
        public double score { get; set; }
        public string path { get; set; }
    }
    
    public class MatchResponse
    {
        public string status { get; set; }

        public int count { get; set; }

        public  Metadata metadata { get; set; }

        public List<MatchDetails> results { get; set; }
    }
    public class Metadata
    {
        public double confidenceScore { get; set; }
    }
    public class MatchDetails
    {
        public string  id { get; set; }
        public double score { get; set; }

        public string path { get; set; }
    }

    

    

}
