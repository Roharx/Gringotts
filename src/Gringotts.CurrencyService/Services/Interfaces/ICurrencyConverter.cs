using Gringotts.Shared.Models;

namespace Gringotts.CurrencyService.Services.Interfaces
{
    public interface ICurrencyConverter
    {
        decimal ConvertToDkk(Money money);
        Money ConvertFromDkk(decimal dkk);
    }
}