using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace MiniBank.Controllers
{
    public class TransferController : Controller
    {

        private readonly IAccountService _accountService;
        private readonly IDashboardService _dashboardService;

        public TransferController(IAccountService accountService,
            IDashboardService dashboardService)
        {
            _accountService = accountService;
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Transfer()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var dashboardData = await _dashboardService.GetDashboardAsync(userId);

            var model = new TransferPageDto
            {
                DemandAccount = dashboardData.DemandAccount,
                TimeDepositAccount = dashboardData.TimeDepositAccount,
                PaymentTypes = dashboardData.PaymentTypes
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferBetweenMyAccounts(TransferBetweenOwnAccountsDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Please check the form fields.";
                return RedirectToAction("Transfer");
            }

            if (!TryParseAmount(dto.Amount, out var amount))
            {
                TempData["ErrorMessage"] = "Please enter a valid amount.";
                return RedirectToAction("Transfer");
            }

            (bool Success, string Message) result;

            switch (dto.TransferDirection)
            {
                case "CURRENT_TO_TIMEDEPOSIT":
                    result = await _accountService.TransferDemandToTimeDepositAsync(userId, amount);
                    break;

                case "TIMEDEPOSIT_TO_CURRENT":
                    result = await _accountService.TransferTimeDepositToDemandAsync(userId, amount);
                    break;

                default:
                    TempData["ErrorMessage"] = "Invalid transfer type.";
                    return RedirectToAction("Transfer");
            }

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction("Transfer");
        }

        private bool TryParseAmount(string? rawAmount, out decimal amount)
        {
            amount = 0;

            if (string.IsNullOrWhiteSpace(rawAmount))
                return false;

            rawAmount = rawAmount.Trim();

            // Sadece nokta varsa: 0.5
            if (rawAmount.Contains('.') && !rawAmount.Contains(','))
            {
                if (decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
                    return amount > 0;

                return false;
            }

            // Sadece virgül varsa: 0,5
            if (rawAmount.Contains(',') && !rawAmount.Contains('.'))
            {
                if (decimal.TryParse(rawAmount, NumberStyles.Number, new CultureInfo("tr-TR"), out amount))
                    return amount > 0;

                return false;
            }

            // Karışık durumlar için normalize et
            rawAmount = rawAmount.Replace(",", ".");

            if (decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
                return amount > 0;

            return false;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferToAnotherUser(TransferByIbanDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Please check the form fields.";
                return RedirectToAction("Transfer");
            }

            if (!TryParseAmount(dto.Amount, out var amount))
            {
                TempData["ErrorMessage"] = "Please enter a valid amount.";
                return RedirectToAction("Transfer");
            }

            var result = await _accountService.TransferToAnotherUserByIbanAsync(
                userId,
                amount,
                dto.ReceiverIban ?? string.Empty,
                dto.ReceiverFirstName ?? string.Empty,
                dto.ReceiverLastName ?? string.Empty,
                dto.Description,
                dto.PaymentTypeId);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction("Transfer");
        }  
    }
}
