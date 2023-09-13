using Azure.Storage.Blobs;
using BlazorDocTextUploader.Server.Interfaces;
using BlazorDocTextUploader.Server.Services;

namespace BlazorDocTextUploader.Server.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration config)
    {
        string connectionString = config.GetConnectionString("Azure:ConnectionString");

        services.AddSingleton(x => new BlobServiceClient(connectionString));
        services.AddScoped<IUploaderService, UploaderService>();

        return services;
    }
}