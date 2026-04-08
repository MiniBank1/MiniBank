namespace Application.DTOs;

public class CardDto
{
    public int CardId { get; set; }
    public int AccountId { get; set; }
    public string CardNumber { get; set; } = null!;
    public DateOnly? ExpiryDate { get; set; }
    public string Cvv { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}