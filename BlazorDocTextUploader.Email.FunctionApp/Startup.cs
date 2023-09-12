using BlazorDocTextUploader.Email.FunctionApp;
using BlazorDocTextUploader.Email.FunctionApp.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace BlazorDocTextUploader.Email.FunctionApp;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        ApplicationServicesExtensions.AddApplicationServices(builder);
    }
}