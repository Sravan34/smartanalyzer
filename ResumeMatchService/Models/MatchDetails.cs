using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResumeMatchService.Models
{
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
        public int  id { get; set; }
        public double score { get; set; }

        public string path { get; set; }
    }

    

    

}
