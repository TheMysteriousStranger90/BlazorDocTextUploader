using BlazorDocTextUploader.Server.Controllers;
using BlazorDocTextUploader.Server.Interfaces;
using BlazorDocTextUploader.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BlazorDocTextUploader.UnitTests.Controllers;

public class UploaderControllerTests
{
    [Fact]
    public async Task Upload_ValidFile_ReturnsOkResult()
    {
        // Arrange
        var model = new DocTextUploaderModel
        {
            UserEmail = "test@example.com",
            UserDocTextFile = CreateMockFormFile()
        };

        var uploaderServiceMock = new Mock<IUploaderService>();
        uploaderServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<DocTextUploaderModel>()))
            .Returns(Task.CompletedTask);

        var controller = new UploaderController(uploaderServiceMock.Object);

        // Act
        var result = await controller.Upload(model) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);


        var responseMessage = result.Value?.GetType().GetProperty("message")?.GetValue(result.Value)?.ToString();
        Assert.Equal("Has been uploaded!", responseMessage);
    }

    [Fact]
    public async Task Upload_InvalidFile_ReturnsNotFoundResult()
    {
        // Arrange
        var model = new DocTextUploaderModel
        {
            UserEmail = "test@example.com",
            UserDocTextFile = CreateMockFormFile("invalid.txt")
        };

        var uploaderServiceMock = new Mock<IUploaderService>();
        uploaderServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<DocTextUploaderModel>()))
            .Throws(new Exception("Invalid file"));

        var controller = new UploaderController(uploaderServiceMock.Object);

        // Act
        var result = await controller.Upload(model) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);


        var errorResponse = result.Value?.GetType().GetProperty("ErrorMessage")?.GetValue(result.Value)?.ToString();
        Assert.Equal("Invalid file", errorResponse);
    }

    private static IFormFile CreateMockFormFile(string fileName = "test.docx")
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("Sample file content");
        writer.Flush();
        stream.Position = 0;

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        return fileMock.Object;
    }
    public class ErrorResponse
    {
        public string ErrorMessage { get; set; }
        public string Trace { get; set; }
    }
}