using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.Services.Interfaces
{
    public interface IAppLogger
    {
        void LogWarning(string message);
        void LogError(string message, Exception exception = null!);
        void LogInformation(string message);
    }
}
