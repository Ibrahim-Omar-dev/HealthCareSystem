using HealthCare.Application.Dto.ImageML;
using HealthCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.Presentation.Controllers;

[ApiController]
[Route("api/image-ml")]
[Authorize]
public class ImageMLController : BaseApiController
{
    private readonly IImageMLService _imageMLService;
    private readonly ILogger<ImageMLController> _logger;


public ImageMLController(
    IImageMLService imageMLService,
    ILogger<ImageMLController> logger)
    {
        _imageMLService = imageMLService;
        _logger = logger;
    }

    [HttpPost("predict")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImagePredictionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Predict(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Image file is required."
            });
        }

        try
        {
            await using var stream = file.OpenReadStream();

            var result = await _imageMLService.PredictAsync(
                stream,
                file.FileName,
                file.ContentType,
                HttpContext.RequestAborted);

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Image ML service error");

            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Image ML service is unavailable or returned an error."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected image prediction error");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Unexpected error while predicting image."
            });
        }
    }


}
