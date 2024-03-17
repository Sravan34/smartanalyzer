namespace SmartAnalyzer.Models
{
    public class FileInfoDetail
    {
        public string id { get; set; }
        public string path { get; set; }

        public List<byte[]> bytes { get; set; }

        public Stream bytesStream { get; set; }
    }
}