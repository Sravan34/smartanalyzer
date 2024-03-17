using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartAnalyzer.Models
{
    public class inputModel
    {
        public string context { get; set; }

        public string category { get; set; }

        public string threshold { get; set; }

        public int noOfMatches { get; set; }

        public string inputPath { get; set; }

    }
}
