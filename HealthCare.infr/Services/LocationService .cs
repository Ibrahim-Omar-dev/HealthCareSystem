using HealthCare.Application.Dto.LocationDTOS;
using HealthCare.Application.Services.Interfaces;
using HealthCare.Domain.Entities;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Infreastructure.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;

        public LocationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, LocationResponseDto? Data)> AddLocationAsync(
            Guid userId, AddLocationDto dto)
        {
            if (dto.Latitude < -90 || dto.Latitude > 90)
                return (false, "Invalid latitude. Must be between -90 and 90.", null);

            if (dto.Longitude < -180 || dto.Longitude > 180)
                return (false, "Invalid longitude. Must be between -180 and 180.", null);

            var existing = await _context.UserLocations
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (existing is not null)
            {
                existing.Latitude = dto.Latitude;
                existing.Longitude = dto.Longitude;
                existing.RecordedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return (true, "Location updated successfully.", MapToDto(existing));
            }

            var location = new UserLocation
            {
                UserId = userId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                RecordedAt = DateTime.UtcNow
            };

            _context.UserLocations.Add(location);
            await _context.SaveChangesAsync();

            return (true, "Location saved successfully.", MapToDto(location));
        }

        public async Task<LocationResponseDto?> GetMyLocationAsync()
        {
            var location = await _context.UserLocations
                .FirstOrDefaultAsync();

            return location is null ? null : MapToDto(location);
        }


        private static LocationResponseDto MapToDto(UserLocation l) => new()
        {
            Id = l.Id,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            RecordedAt = l.RecordedAt
        };
    }
}
