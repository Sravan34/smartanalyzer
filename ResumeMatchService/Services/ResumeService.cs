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

namespace ResumeMatchService.Services
{
    //IResumeService :ResumeService {

    //  GetResumeMatchRecords();

    //  }
    public class ResumeService
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("02e06cff2f1648cb907a656830c9cf6d");
        private static readonly Uri endpoint = new Uri("https://resumetextextraction.cognitiveservices.azure.com/");
        private  int matchperc = 0;
        private string candidateName;
        public CandDetails GetResumeMatchRecords(string docPath,string fileName,string domain,string skill)
        {
            //FileInfo fi = new FileInfo(docPath + "\\" +fileName);
            //string docactPath = Directory.GetFileSystemEntries(docPath).Where(x => x.Contains(fileName)).FirstOrDefault();
            //var ext = Path.GetExtension(fileName).ToLowerInvariant();
            string extractedText = "";
            var client = new TextAnalyticsClient(endpoint, credentials);
            if (docPath.Contains(".pdf"))
                extractedText += ReadTextfromPdfFile(docPath);
            else if(docPath.Contains(".docx"))
                extractedText += ReadTextfromFile(docPath);
            var matchPerc = ExtractKeys(client, extractedText, domain, skill);
            CandDetails cd = new CandDetails();
            cd.CandId = 1;
            cd.candName = "Test";
            cd.fileName = fileName;
            cd.primarySkill = domain;
            cd.weightage = matchPerc;
            cd.weightageDetails = skill;
            return cd;
        }

        public  int ExtractKeys(TextAnalyticsClient client, string text, string domain, string skill)
        {
            Dictionary<string, string> candDict = new Dictionary<string, string>();
            List<string> lstkeys = new List<string>();
            Dictionary<string, string> extractedSkills = new Dictionary<string, string>();
            if (text.Length > 5000)
            {
                var textsplitted2 = TextSplit.SplitText(text, 5000).ToList();
                foreach (string str in textsplitted2)
                {
                    var response = client.RecognizeEntities(str).Value;
                    if (response.Any(x => x.Text.Contains(domain)) && !extractedSkills.ContainsValue(domain))
                    {
                        extractedSkills.Add("Domain", domain);
                        matchperc += 10;
                    }
                    if (response.Any(x => x.Text.Contains(skill)) && !extractedSkills.ContainsValue(skill))
                    {
                        extractedSkills.Add("Skill", skill);
                        matchperc += 10;
                    }

                }

               
            }
            else
            {
                text = text.Replace("Evaluation Warning: The document was created with Spire.Doc for .NET.","");
                var response1 = client.RecognizeEntities(text).Value;
                var response2 = client.ExtractKeyPhrases(text).Value;
                if (response1.Any(x => x.Text.ToLower().Contains(domain.ToLower())))
                     matchperc += 10;
                if(response1.Any(x => x.Text.ToLower().Contains(skill.ToLower())))
                    matchperc += 10;
            }
            //return lstkeys;
            return matchperc;
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
