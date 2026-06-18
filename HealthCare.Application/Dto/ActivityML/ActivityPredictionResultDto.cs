using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.DTOs.ActivityML;

public class ActivityPredictionResultDto
{
    public bool Success { get; set; }
    public string Activity { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int ReadingsCount { get; set; }
}