using BlazorDocTextUploader.Email.FunctionApp.Models;

namespace BlazorDocTextUploader.Email.FunctionApp.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailModel email);
}