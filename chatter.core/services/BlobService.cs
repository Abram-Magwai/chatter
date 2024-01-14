using Azure.Storage.Blobs;
using chatter.core.interfaces;
using chatter.view.models;
using Microsoft.Extensions.Options;

namespace chatter.core.services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobClient;
        private readonly BlobContainerClient _containerClient;

        public BlobService(BlobServiceClient blobServiceClient, IOptions<AzureSettings> options)
        {
            _blobClient = blobServiceClient;
            string blobContainerName = options.Value.ContainerName;
            _containerClient = _blobClient.GetBlobContainerClient(blobContainerName);
            _containerClient.CreateIfNotExists();
        }
        public async Task AddBlob(string filePath, string fileName, Stream stream)
        {
            string fullFilePath = Path.Combine(filePath, fileName);
            await _containerClient.UploadBlobAsync(fullFilePath, stream);
        }
        public async Task DeleteBlob(string name)
        {
            await _containerClient.DeleteBlobAsync(name);
        }
        public string GetBlob(string fileName)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }
    }
}