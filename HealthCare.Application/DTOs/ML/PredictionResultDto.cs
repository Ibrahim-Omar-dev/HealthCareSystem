using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.DTOs.ML;

public class RiskFactorDto
{
    public string Feature { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Impact { get; set; }
    public string Direction { get; set; } = string.Empty;
}

public class PredictionResultDto
{
    public string Status { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<RiskFactorDto> RiskFactors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}