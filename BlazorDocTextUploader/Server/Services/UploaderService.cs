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
    private readonly IConfiguration _configuration;
    private string blobStorageConnection = string.Empty;
    private string blobContainerName = string.Empty;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureOptions _azureOptions;

    public UploaderService(IConfiguration configuration, BlobServiceClient blobServiceClient, IOptions<AzureOptions> azureOptions)
    {
        _blobServiceClient = blobServiceClient;

        _azureOptions = azureOptions.Value;

        blobStorageConnection = _configuration.GetConnectionString("Azure:ConnectionString");
        blobContainerName = _configuration.GetConnectionString("Azure:Container");
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
        var container = new BlobContainerClient(_azureOptions.ConnectionString, _azureOptions.Container);
        var createResponse = await container.CreateIfNotExistsAsync();

        if (createResponse != null && createResponse.GetRawResponse().Status == 201)
            await container.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

        var fileBuffer = new MemoryStream();

        await model
            .UserDocTextFile
            .CopyToAsync(fileBuffer);

        var blobClient = container.GetBlobClient(model.UserDocTextFile.FileName);

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

        using var readUploadFile = model.UserDocTextFile.OpenReadStream();

        await blobClient.UploadAsync(readUploadFile, overwrite: false);
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