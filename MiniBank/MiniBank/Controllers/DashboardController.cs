using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiniBank.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IAccountService _accountService;

    public DashboardController(
        IDashboardService dashboardService,
        IAccountService accountService)
    {
        _dashboardService = dashboardService;
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await _accountService.ApplyDailyInterestIfNeededAsync(userId);

        var model = await _dashboardService.GetDashboardAsync(userId);
        return View(model);
    }
}