using BlazorDocTextUploader.Email.FunctionApp;
using BlazorDocTextUploader.Email.FunctionApp.Interfaces;
using BlazorDocTextUploader.Email.FunctionApp.Models;
using BlazorDocTextUploader.Email.FunctionApp.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace BlazorDocTextUploader.Email.FunctionApp;

public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.Configure<EmailSettings>(builder.GetContext().Configuration.GetSection("EmailSettings"));
        builder.Services.AddSingleton<IEmailService, EmailService>();
    }
}