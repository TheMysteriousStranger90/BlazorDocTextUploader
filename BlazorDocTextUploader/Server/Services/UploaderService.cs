using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlazorDocTextUploader.Server.Interfaces;
using BlazorDocTextUploader.Server.Models;
using BlazorDocTextUploader.Shared.Models;
using Microsoft.Extensions.Options;

namespace BlazorDocTextUploader.Server.Services;

public class UploaderService : IUploaderService
{
    
    private const string _blobContainerName = "projectcontainer";
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureOptions account;

    public UploaderService(BlobServiceClient blobServiceClient, IOptions<AzureOptions> config)
    {
        _blobServiceClient = blobServiceClient;

        account.Account = config.Value.Account;
        account.Container = config.Value.Container;
        account.ConnectionString = config.Value.ConnectionString;
        account.ResourceGroup = config.Value.ResourceGroup;
    }

    public async Task UploadFileAsync(DocTextUploaderModel model)
    {
        if (IsDoc(model.UserDocTextFile))
        {
            await UploadDocxAsync(model);
        }
    }

    private async Task UploadDocxAsync(DocTextUploaderModel model)
    {
        BlobContainerClient? docsContainer = await EnsureBlobContainer(_blobServiceClient);

        if (docsContainer == null) return;

        var fileBuffer = new MemoryStream();

        await model
            .UserDocTextFile
            .CopyToAsync(fileBuffer);

        var blobClient = docsContainer.GetBlobClient(model.UserDocTextFile.FileName);

        using var readUploadFile = model.UserDocTextFile.OpenReadStream();
        await blobClient.UploadAsync(readUploadFile, overwrite: false);
    }

    private async Task<BlobContainerClient?> EnsureBlobContainer(BlobServiceClient blobServiceClient)
    {
        try
        {
            
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(_blobContainerName);

            container.CreateIfNotExists(PublicAccessType.BlobContainer);

            if (await container.ExistsAsync())
            {
                Console.WriteLine("Created container {0}", container.Name);
            }

            return container;
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine("HTTP error code {0}: {1}", e.Status, e.ErrorCode);
            Console.WriteLine(e.Message);
        }

        return null;
    }

    public static bool IsDoc(IFormFile file)
    {
        if (file.ContentType.Contains("application/msword") ||
            file.ContentType.Contains("application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
        {
            return true;
        }

        string[] allowedExtensions = { ".doc", ".docx" };
        string fileExtension = System.IO.Path.GetExtension(file.FileName);

        return allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
    }
}