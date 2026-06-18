using HealthCare.Application.Dto.LocationDTOS;

namespace HealthCare.Application.Services.Interfaces
{
    public interface ILocationService
    {
        Task<(bool Success, string Message, LocationResponseDto? Data)> AddLocationAsync(Guid userId, AddLocationDto dto);
        Task<LocationResponseDto?> GetMyLocationAsync();
    }
}
