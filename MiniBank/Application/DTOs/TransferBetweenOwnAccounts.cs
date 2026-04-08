using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class TransferBetweenOwnAccountsDto
{
    [Required(ErrorMessage = "Please select the transfer direction.")]
    public string? TransferDirection { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    public string? Amount { get; set; }
}