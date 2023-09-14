using System;
using System.IO;
using System.Text;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using BlazorDocTextUploader.SendEmail.FunctionApp.Models.Enums;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BlazorDocTextUploader.SendEmail.FunctionApp;

public class BlobTriggerFunction
{
    private readonly SmtpClient _smtpClient;
    private readonly ILogger<BlobTriggerFunction> _logger;

    public BlobTriggerFunction(SmtpClient smtpClient, ILogger<BlobTriggerFunction> logger)
    {
        _smtpClient = smtpClient ?? throw new ArgumentNullException(nameof(smtpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [FunctionName("BlobTriggerFunction")]
    public async Task Run(
        [BlobTrigger("projectcontainer/{name}", Connection = "AzureWebJobsStorage")]
        byte[] blobContent,
        string name, ILogger log)
    {
        try
        {
            var (userEmail, docxName) = ParseBlobName(name);
            var sasToken = GenerateSasToken(name);

            var emailMessage = CreateEmailMessage(userEmail, sasToken);

            await _smtpClient.ConnectAsync("smtp.sendgrid.net", 587, SecureSocketOptions.StartTls);
            await _smtpClient.AuthenticateAsync(
                userName: Environment.GetEnvironmentVariable("UserNameGrid"),
                password: Environment.GetEnvironmentVariable(
                    "SendGridApiKey")
            );
            await _smtpClient.SendAsync(emailMessage);
            await _smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing blob: {ex}");
        }
    }
    
    private (string userEmail, string docxName) ParseBlobName(string name)
    {
        var receivedFilenamePieces = name.Split("__");
        if (receivedFilenamePieces.Length >= 2)
        {
            return (receivedFilenamePieces[0], receivedFilenamePieces[1]);
        }
        throw new ArgumentException("Invalid blob name format");
    }
    
    private string GenerateSasToken(string blobName)
    {
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        
        string containerName = "projectcontainer";
        
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        
        BlobSasBuilder sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
            Protocol = SasProtocol.Https,
            StartsOn = DateTimeOffset.UtcNow
        };
        
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        
        string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(
            Environment.GetEnvironmentVariable("YourStorageAccountName"),
            Environment.GetEnvironmentVariable("YourStorageAccountKey"))).ToString();
        
        string protectedUrl = blobClient.Uri + "?" + sasToken;

        return protectedUrl;
    }
    
    private MimeMessage CreateEmailMessage(string userEmail, string sasToken)
    {
        using var email = new MimeMessage();
        email.From.Add(new MailboxAddress(
            "Bohdan",
            "bohdan_harabadzhyu@outlook.com"
        ));
        email.To.Add(new MailboxAddress(
            "userEmail",
            "userEmail"
        ));
        email.Subject = "File Uploaded Notification";
        
        var textPart = new TextPart(TextFormat.Plain)
        {
            Text = "Your file has been successfully uploaded."
        };

        var htmlPart = new TextPart(TextFormat.Html)
        {
            Text = $@"<html>
                    <body>
                        <p>Your file has been successfully uploaded.</p>
                        <p>You can access it using the following link:</p>
                        <p><a href=""{sasToken}"">Download Link</a></p>
                    </body>
                  </html>"
        };
        
        var multipart = new Multipart("alternative");
        multipart.Add(textPart);
        multipart.Add(htmlPart);
        
        email.Body = multipart;

        return email;
    }
}

    
    
