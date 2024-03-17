namespace SmartAnalyzer.Models
{
    public class SearchRequest
    {
        public string context { get; set; }
        public string category { get; set; }
        public decimal threshold { get; set; }
        public int noOfMatches { get; set; }
        public string inputFilePath { get; set; }
    }
}
