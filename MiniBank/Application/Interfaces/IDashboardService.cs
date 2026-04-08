using Application.DTOs;

namespace Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(int userId);
}