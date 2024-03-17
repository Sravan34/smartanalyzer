using Azure;
using Azure.AI.TextAnalytics;
using SmartAnalyzer.Models;
using SmartAnalyzer.Utils;
using Spire.Doc;
using Spire.Pdf.Exporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Pdf.Exporting.Text;
namespace SmartAnalyzer.Services
{
    public class ResumeService
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("ed6b89f9b604433ea8e2cdeafef2e3ab");
        //private static readonly AzureKeyCredential credentials = new AzureKeyCredential(AppSettings["EndPointUri"]);
        private static readonly Uri endpoint = new Uri("https://smartanalyzer.cognitiveservices.azure.com/");
        private double matchperc = 0;
        Dictionary<string, double> dictConfidence = new Dictionary<string, double>();
        Dictionary<string, double> confScoredoc = new Dictionary<string, double>();

        public List<MatchResponse> GetResumeMatchRecords(SearchStorageRequest searchStorageRequest)
        {
            List<MatchResponse> lstresMatch = new List<MatchResponse>();
            List<MatchDetails> lstmatchdet = new List<MatchDetails>();
            List<CategorizedEntityCollection> lstkeyresume = new List<CategorizedEntityCollection>();
            string extractedText = "";
            double confidenceScore = 0;
            List<string> lstkeysresume = new List<string>();
            List<string> lstjdkeys = new List<string>();
            var client = new TextAnalyticsClient(endpoint, credentials);
            lstjdkeys = ExtractKeys2(client, searchStorageRequest.SearchRequest.context);
            
            int i = 1;
            foreach (var fileInfo in searchStorageRequest.FileInfoDetail) {

                extractedText = "";
                if (fileInfo.id.Contains(".pdf"))
                    extractedText += ReadTextfromPdfFile(fileInfo);
                else if (fileInfo.id.Contains(".docx"))
                    extractedText += ReadTextfromFile(fileInfo);

                lstkeysresume = ExtractKeys2(client, extractedText);
                foreach (var lst in lstkeyresume)
                {
                    lstkeysresume.Add(lst.Select(x => x.Text).ToString());
                }
                List<string> items = lstkeysresume.Intersect(lstjdkeys).ToList();
                if (items.Count != 0)
                {
                    matchperc = (double)items.Count / lstjdkeys.Count;
                    matchperc = Math.Round(matchperc, 2);
                    lstmatchdet.Add(new MatchDetails { id = fileInfo.id, score = matchperc, path = fileInfo.path });
                    i++;
                }
            }
           
            if (lstmatchdet.Count != 0)
            {
                var result = lstmatchdet.OrderByDescending(x => x.score).Take(searchStorageRequest.SearchRequest.noOfMatches);

                if (result.Count() > 1)
                {
                    foreach (var item in result)
                    {

                        confidenceScore = confidenceScore + item.score;
                    }
                    confidenceScore = Math.Round((double)((confidenceScore * 100) / result.Count()), 2);

                }
                else
                {
                    confidenceScore = result.Select(x => x.score).FirstOrDefault();
                }

                lstresMatch.Add(new MatchResponse { status = "Success", count = result.Count(), metadata = new Metadata { confidenceScore = confidenceScore }, results = result.ToList() });

            }
            return lstresMatch;

        }

        public List<CategorizedEntityCollection> ExtractKeys(TextAnalyticsClient client, string text)
        {
            List<CategorizedEntityCollection> catlst = new List<CategorizedEntityCollection>();
            if (text.Length > 5000)
            {
                text = text.Replace("Evaluation Warning: The document was created with Spire.Doc for .NET.", "");
                var textsplitted2 = TextSplit.SplitText(text, 5000).ToList();

                foreach (string str in textsplitted2)
                {
                    catlst.Add(client.RecognizeEntities(str).Value);
                }
            }
            else
            {
                text = text.Replace("Evaluation Warning: The document was created with Spire.Doc for .NET.", "");
                catlst.Add(client.RecognizeEntities(text).Value);

            }
            return catlst;
        }


        public List<string> ExtractKeys2(TextAnalyticsClient client, string text)
        {
            List<string> cat = new List<string>();
            if (text.Length > 5000)
            {
                text = text.Replace("Evaluation Warning: The document was created with Spire.Doc for .NET.", "");
                var textsplitted2 = TextSplit.SplitText(text, 5000).ToList();

                foreach (string str in textsplitted2)
                {
                    var valu = client.RecognizeEntities(str).Value;

                    foreach (var val in valu)
                    {
                        if (!(dictConfidence.Keys.Contains(val.Text)))
                            dictConfidence.Add(val.Text, val.ConfidenceScore);
                        cat.Add(val.Text);
                    }

                }
            }
            else
            {
                text = text.Replace("Evaluation Warning: The document was created with Spire.Doc for .NET.", "");
                var val1 = client.RecognizeEntities(text).Value;
                cat = val1.Select(x => x.Text).ToList();
            }
            return cat;
        }

        public string ReadTextfromFile(FileInfoDetail FileInfoDetail)
        {
            Document doc = new Document();
            doc.LoadFromStream(FileInfoDetail.bytesStream,FileFormat.Doc);
            string text1 = doc.GetText();
            return text1;
        }
               
        public string ReadTextfromPdfFile(FileInfoDetail FileInfoDetail)
        {
            SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
            StringBuilder str = new StringBuilder();
            Spire.Pdf.PdfDocument pdfDoc = new Spire.Pdf.PdfDocument();
            
            pdfDoc.LoadFromStream(FileInfoDetail.bytesStream);
            foreach (Spire.Pdf.PdfPageBase page in pdfDoc.Pages)
            {
                str.Append(page.ExtractText(strategy));
            }
            return str.ToString();
        }
    }
}
