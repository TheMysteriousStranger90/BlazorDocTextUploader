using BlazorDocTextUploader.Server.Interfaces;
using BlazorDocTextUploader.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlazorDocTextUploader.Server.Controllers;

public class UploaderController : BaseApiController
{
    private readonly IUploaderService _uploaderService;

    public UploaderController(IUploaderService uploaderService)
    {
        _uploaderService = uploaderService;
    }
    
    [HttpPost("projectcontainer")]
    public async Task<IActionResult> Upload([FromForm] DocTextUploaderModel model)
    {
        try
        {
            await _uploaderService.UploadFileAsync(model);
        
            return Ok(new { message = "Has been uploaded!" });
        }
        catch (Exception e)
        {
            return NotFound(new
            {
                ErrorMessage = e.Message,
                Trace = e.StackTrace
            });
        }
    }
}