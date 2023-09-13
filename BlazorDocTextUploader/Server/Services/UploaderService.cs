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
    private readonly AzureOptions _azureOptions;

    public UploaderService(IOptions<AzureOptions> azureOptions)
    {
        _azureOptions = azureOptions.Value;
    }

    public async Task UploadFileAsync(DocTextUploaderModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.UserEmail) || model.UserDocTextFile == null)
        {
            throw new ArgumentException("UserEmail and UserDocTextFile are required.");
        }

        if (!IsDoc(model.UserDocTextFile))
        {
            throw new ArgumentException("Invalid file type. Only .doc and .docx files are allowed.");
        }

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

    private bool IsDoc(IFormFile file)
    {
        if (file.ContentType.Contains("application/msword") ||
            file.ContentType.Contains("application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
        {
            return true;
        }

        string[] allowedExtensions = { ".doc", ".docx" };
        string fileExtension = Path.GetExtension(file.FileName);

        return allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
    }
}