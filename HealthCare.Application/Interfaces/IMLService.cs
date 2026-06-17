using System;
using System.Collections.Generic;
using System.Text;
using HealthCare.Application.Dto.ML;

namespace HealthCare.Application.Interfaces;

public interface IMLService
{
    /// <summary>
    /// Fetches bpm, spo2, resp_rate, temp from DB for the given patient,
    /// prepends heart_rate = 70 fixed, and returns the ordered list.
    /// </summary>
    Task<VitalsListDto> GetVitalsForPredictionAsync(Guid patientId);

    /// <summary>
    /// Sends the vitals list to the Python ML service and returns the prediction.
    /// </summary>
    Task<PredictionResultDto> PredictHealthRiskAsync(VitalsListDto vitals);
}