namespace Gringotts.Shared.Models.CurrencyService;

public class BalanceUpdateRequest
{
    public Guid UserId { get; set; }
    public decimal DkkAmount { get; set; }
    public int Galleons { get; set; }
    public int Sickles { get; set; }
    public int Knuts { get; set; }
}