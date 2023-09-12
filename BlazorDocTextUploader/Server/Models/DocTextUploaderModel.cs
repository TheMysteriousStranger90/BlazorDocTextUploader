namespace BlazorDocTextUploader.Server.Models;

public class DocTextUploaderModel
{
    public string UserEmail { get; set; }
    public IFormFile UserDocTextFile { get; set; }
}