using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using Azure;
using Spire.Pdf.Exporting.Text;
using System.Text;
using Spire.Pdf;
using ResumeMatchServices.Utils;
using ResumeMatchServices.Models;
using System.IO;
using Spire.Doc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using ResumeMatchService.Models;
using System.Configuration;

namespace ResumeMatchService.Services
{
   
    public class ResumeService
    {
        
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("ed6b89f9b604433ea8e2cdeafef2e3ab");
        //private static readonly AzureKeyCredential credentials = new AzureKeyCredential(AppSettings["EndPointUri"]);
        private static readonly Uri endpoint = new Uri("https://smartanalyzer.cognitiveservices.azure.com/");
        private  double matchperc = 0;
        Dictionary<string, double> dictConfidence = new Dictionary<string, double>();
        Dictionary<string, double> confScoredoc = new Dictionary<string, double>();
        public List<MatchResponse> GetResumeMatchRecords(string context, string category, double threshold, int noOfMatches, string inputPath)
        {
            List<MatchResponse> lstresMatch = new List<MatchResponse>();
            List<MatchDetails> lstmatchdet = new List<MatchDetails>();
            List<CategorizedEntityCollection> lstkeyresume = new List<CategorizedEntityCollection>();
            string extractedText = "";
            double confidenceScore = 0;
            List<string> lstkeysresume = new List<string>();
            List<string> lstjdkeys = new List<string>();
            var client = new TextAnalyticsClient(endpoint, credentials);
            lstjdkeys = ExtractKeys2(client,context);
            if (Directory.Exists(inputPath))
            {
                int i = 1;
                foreach (var file in Directory.GetFiles(inputPath))
                {
                    extractedText = "";
                    var ext = Path.GetExtension(file);
                    if (ext.Contains(".pdf"))
                        extractedText += ReadTextfromPdfFile(file);
                    else if (ext.Contains(".docx"))
                        extractedText += ReadTextfromFile(file);
                    lstkeysresume = ExtractKeys2(client, extractedText);
                    foreach(var lst in lstkeyresume)
                    {
                        lstkeysresume.Add(lst.Select(x => x.Text).ToString());
                    }
                    List<string> items = lstkeysresume.Intersect(lstjdkeys).ToList();
                    if (items.Count != 0)
                    {
                        //foreach (var item in items)
                        //{
                        //    if (dictConfidence.Any(x => x.Key.ToString() == item))
                        //    {
                        //        var score = dictConfidence.Where(x => x.Key.ToString() == item).Select(x => x.Value).FirstOrDefault();
                        //        //if(!confScoredoc.ContainsKey(file))
                        //        //    confScoredoc.Add(file, score);
                        //    }
                        //}
                        matchperc = (double)items.Count / lstjdkeys.Count;
                        matchperc = Math.Round(matchperc, 2);
                        lstmatchdet.Add(new MatchDetails { id = i, score = matchperc, path = file });
                        i++;
                    }
                }

                
            }
            //foreach(var key in confScoredoc)
            //{
               
            //        confidenceScore += Convert.ToDouble(key.Value);
            //}
            //if (confScoredoc.Count > 1)
            //{
            //    confidenceScore = Math.Round((double)((confidenceScore * 100) / confScoredoc.Count), 2);
            //}
            if (lstmatchdet.Count != 0)
            {
                var result = lstmatchdet.OrderByDescending(x => x.score).Take(noOfMatches);

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
                
                lstresMatch.Add(new MatchResponse { status = "Success", count = result.Count(), metadata = new Metadata {confidenceScore = confidenceScore}, results = result.ToList() });

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
                    
                    foreach(var val in valu)
                    {
                        if(!(dictConfidence.Keys.Contains(val.Text)))
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

        
        public string ReadTextfromFile(string path)
        {
            Document doc = new Document();
            doc.LoadFromFile(path);
            string text1 = doc.GetText();
            return text1;
        }

        public string ReadTextfromPdfFile(string path)
        {
            SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
            StringBuilder str = new StringBuilder();
            PdfDocument pdfDoc = new PdfDocument();
            pdfDoc.LoadFromFile(path);
            foreach (PdfPageBase page in pdfDoc.Pages)
            {
                str.Append(page.ExtractText(strategy));
            }
            return str.ToString();
        }

    }
}
