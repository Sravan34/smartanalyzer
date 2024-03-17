
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SmartAnalyzer.Models;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using System.Globalization;
using Azure.Storage;
using System.Reflection.Metadata;
using HarfBuzzSharp;
using static System.Reflection.Metadata.BlobBuilder;
using System.Collections.Generic;
namespace SmartAnalyzer.Services
{
    public class BlobContainerService : IBlobContainerService
    {
        private readonly BlobServiceClient _blobServiceClient;
        public BlobContainerService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }
        private async Task<string> GetFileContentAsyncREST()
        {
            string storageAccountName = "hackathontestdata2024";
            string storageAccountKey = "Ay1jvnupCShhBmo5ky/j2IPeMPBCkIxYP8gZ6RVAdKi6RgBlMdWBoylkJ/67BDU5DzCQkwRComSV+AStxoGWGQ==";
            String uri = string.Format("http://{0}.blob.core.windows.net?comp=list", "hackathontestdata2024");
            Byte[] requestPayload = null;

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload)
            })
            {
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", "2024-03-17");
                httpRequestMessage.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(
                       storageAccountName, storageAccountKey, now, httpRequestMessage);

                using (HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(httpRequestMessage, CancellationToken.None))
                {
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        String xmlString = await httpResponseMessage.Content.ReadAsStringAsync();
                        XElement x = XElement.Parse(xmlString);
                        foreach (XElement container in x.Element("Containers").Elements("Container"))
                        {
                            return container.Element("Name").Value;
                        }

                    }
                }
            }

            return string.Empty;
        }
    
        public async Task<List<string>> GetAllContainer()
        {
            List<string> containerName = new();

            // TODO via Rest API is failing
            //string res = await GetFileContentAsyncREST();
            //containerName.Add(res);

            await foreach (BlobContainerItem blobkContainerItem in _blobServiceClient.GetBlobContainersAsync())
            {
                containerName.Add(blobkContainerItem.Name);
            }

            return containerName;
        }
        public async Task<List<MatchResponse>> GetAllBlobsByContainer(SearchRequest searchRequest)
        {
            SearchStorageRequest searchStorageRequest = new SearchStorageRequest();
            List<FileInfoDetail> fileInfoDetail = new List<FileInfoDetail>();

            List<string> containerNames = new();
            containerNames = await GetAllContainer();

            foreach (string containerName in containerNames)
            {
                BlobContainerClient _blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
                await foreach (BlobItem blobItem in _blobContainer.GetBlobsAsync())
                {
                    var blobClient = _blobContainer.GetBlobClient(blobItem.Name);
                    var streamObject = await blobClient.OpenReadAsync();
                    fileInfoDetail.Add(new FileInfoDetail() { bytesStream = streamObject, id = blobItem.Name, path = string.Format("https://{0}.blob.core.windows.net/{1}/{2}", "smartanalyzerstoragein",containerName, blobItem.Name) });
                }
            }

            searchStorageRequest.SearchRequest = searchRequest;
            searchStorageRequest.FileInfoDetail = fileInfoDetail;

            //TODO call analyzer 
            ResumeService rs = new ResumeService();
            List<MatchResponse>  matchResponse= rs.GetResumeMatchRecords(searchStorageRequest);

            // TODO push the results files to storage
            // await CreateContainer("results");
            return matchResponse;
        }

        public async Task CreateContainer(string containerName)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

        }

        public async Task<bool> UploadBlob(string name, Stream file, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(name);

            var httpHeaders = new BlobHttpHeaders()
            {
                ContentType = "application/json"
            };

            IDictionary<string, string> metadata =
             new Dictionary<string, string>();

            metadata.Add("title", name);
            metadata["comment"] = name;

            var result = await blobClient.UploadAsync(file, httpHeaders, metadata);

            //metadata.Remove("title");

            //await blobClient.SetMetadataAsync(metadata);

            if (result != null)
            {
                return true;
            }
            return false;
        }
    }
}
