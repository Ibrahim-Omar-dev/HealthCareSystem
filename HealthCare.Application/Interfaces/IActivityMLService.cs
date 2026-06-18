using System;
using System.Collections.Generic;
using System.Text;
using HealthCare.Application.DTOs.ActivityML;

namespace HealthCare.Application.Interfaces;

public interface IActivityMLService
{
    Task<ActivityPredictionResultDto> PredictAsync(
        List<List<double>> readings,
        CancellationToken cancellationToken = default);
}