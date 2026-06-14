using System;
using System.Collections.Generic;
using System.Text;

using HealthCare.Application.DTOs.ImageML;

namespace HealthCare.Application.Interfaces;

public interface IImageMLService
{
    Task<ImagePredictionResultDto> PredictAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}