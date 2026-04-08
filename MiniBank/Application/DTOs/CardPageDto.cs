namespace Application.DTOs;

public class CardPageDto
{
    public CardDto? ActiveCard { get; set; }
    public bool HasActiveCard => ActiveCard != null;
}