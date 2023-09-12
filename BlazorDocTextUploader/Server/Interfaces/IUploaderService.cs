using BlazorDocTextUploader.Server.Models;

namespace BlazorDocTextUploader.Server.Interfaces;

public interface IUploaderService
{
    Task UploadFileAsync(DocTextUploaderModel model);
}