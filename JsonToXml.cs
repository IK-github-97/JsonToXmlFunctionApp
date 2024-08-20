using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace YourNamespace
{
    public class JsonToXml
    {
        public bool connectionString { get; set; }

        [FunctionName("JsonToXml")]
        public void Run([BlobTrigger("jsonfiles/{name}")] Stream myBlob, string name, ILogger log, IConfiguration configuration)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            try
            {
                getKeyVaultInfo(configuration);
                // Create a BlobServiceClient using the retrieved connection string
                BlobServiceClient blobServiceClient = new BlobServiceClient(Convert.ToString(connectionString));

                // Process the blob
                using (StreamReader reader = new StreamReader(myBlob))
                {
                    string jsonContent = reader.ReadToEnd();
                    dynamic jsonObj = JsonConvert.DeserializeObject(jsonContent);
                    XmlDocument xmlDoc = JsonConvert.DeserializeXmlNode(jsonObj);

                    string xmlFilePath = $"convertedxmlfiles/{name}.xml";
                    xmlDoc.Save(xmlFilePath);

                    log.LogInformation($"JSON converted to XML and saved as {xmlFilePath}");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error converting JSON to XML: {ex.Message}");
            }
        }

        public KeyVaultSecret getKeyVaultInfo(IConfiguration configuration)
        {
            // Retrieve connection string from Key Vault
            var keyVaultUri = configuration["AzureKeyVaultUrl"];
            var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            var secret = secretClient.GetSecret("StorageConnectionString");
            var connectionString = secret.Value;
            return connectionString;
        }
    }
}
