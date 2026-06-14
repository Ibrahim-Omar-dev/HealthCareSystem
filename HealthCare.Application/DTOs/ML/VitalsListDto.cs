using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.DTOs.ML;

/// <summary>
/// The raw vitals list returned from GetVitalsForPrediction
/// and consumed by PredictHealthRisk.
/// Order: [heart_rate, bpm, spo2, resp_rate, temp]
/// </summary>
public class VitalsListDto
{
    /// <summary>
    /// Ordered vitals: heart_rate=70 fixed, bpm, spo2, resp_rate, temp
    /// </summary>
    public List<double> Vitals { get; set; } = new();
}