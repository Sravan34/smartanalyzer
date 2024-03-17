namespace SmartAnalyzer.Models
{
    public class SearchStorageRequest
    {
        public List<FileInfoDetail> FileInfoDetail { get; set; }

        public SearchRequest SearchRequest { get; set; }
    }
}
