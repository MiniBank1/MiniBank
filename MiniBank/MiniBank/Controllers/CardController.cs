using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MiniBank.Controllers;

[Authorize]
public class CardController : Controller
{
    private readonly ICardService _cardService;

    public CardController(ICardService cardService)
    {
        _cardService = cardService;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var model = await _cardService.GetMyCardPageAsync(userId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create()
    {
        var userId = GetUserId();
        var result = await _cardService.CreateCardForCurrentAccountAsync(userId);

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Index", "Account");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate()
    {
        var userId = GetUserId();
        var result = await _cardService.DeactivateMyCardAsync(userId);

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Index", "Account");
    }
}