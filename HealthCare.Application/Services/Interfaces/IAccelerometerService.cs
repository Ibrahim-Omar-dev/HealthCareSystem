using HealthCare.Application.Dto.Accelerometer;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.Services.Interfaces
{
    public interface IAccelerometerService
    {
        Task<(bool Success, string Message)> AddReadingsAsync(AddAccelerometerDto dto);

        Task<object?> GetMyReadingsAsync();

    }
}
