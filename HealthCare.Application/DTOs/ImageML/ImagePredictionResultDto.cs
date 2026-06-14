using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.DTOs.ImageML;

public class ImagePredictionResultDto
{
    public bool Success { get; set; }
    public string UploadedPath { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string RawResult { get; set; } = string.Empty;
}