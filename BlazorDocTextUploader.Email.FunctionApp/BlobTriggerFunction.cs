using System;
using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using BlazorDocTextUploader.Email.FunctionApp.Interfaces;
using BlazorDocTextUploader.Email.FunctionApp.Models;
using BlazorDocTextUploader.Email.FunctionApp.Models.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlazorDocTextUploader.Email.FunctionApp;

public class BlobTriggerFunction
{
    private readonly IEmailService _emailService;

    public BlobTriggerFunction(IEmailService emailService)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    [Function("BlobTriggerFunction")]
    public async Task Run(
        [BlobTrigger("projectcontainer/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream,
        string name,
        FunctionContext context)
    {
        var log = context.GetLogger<BlobTriggerFunction>();
        
        try
        {
            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var containerClient = blobServiceClient.GetBlobContainerClient("projectcontainer");
            var blobClient = containerClient.GetBlobClient(name);
            
            var receivedFilenamePieces = name.Split("__");
            string userEmail = receivedFilenamePieces[(int)FilenamePieces.UserEmail];
            string docxName = receivedFilenamePieces[(int)FilenamePieces.UserDocTextFile];
            
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = name,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                Protocol = SasProtocol.Https
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = blobClient.GenerateSasUri(sasBuilder).ToString();
            
            var emailModel = new EmailModel
            {
                To = userEmail,
                Subject = "File Successfully Uploaded",
                Body = $"<p>Dear <strong>{userEmail}</strong>, Your file '{docxName}' has been successfully uploaded. You can access it <a href='{sasToken}'>here</a>.</p>"
            };

            await _emailService.SendEmailAsync(emailModel);

            log.LogInformation($"Email sent to {emailModel.To}");
        }
        catch (Exception ex)
        {
            log.LogError($"Error processing blob: {ex.Message}");
        }
    }
}
