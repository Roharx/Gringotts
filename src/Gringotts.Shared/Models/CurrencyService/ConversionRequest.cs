namespace Gringotts.Shared.Models.CurrencyService;

public class ConversionRequest
{
    public string Direction { get; set; } = "ToWizard"; // or "ToDkk"
    public decimal Amount { get; set; }
}