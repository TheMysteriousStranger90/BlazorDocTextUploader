using BlazorDocTextUploader.Email.FunctionApp.Interfaces;
using BlazorDocTextUploader.Email.FunctionApp.Models;
using BlazorDocTextUploader.Email.FunctionApp.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDocTextUploader.Email.FunctionApp.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(IFunctionsHostBuilder functionsHostBuilder)
    {
        //var config = functionsHostBuilder.GetContext().Configuration;
        var config = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        
        functionsHostBuilder.Services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        
        return functionsHostBuilder.Services.AddScoped<IEmailService, EmailService>();
    }
}