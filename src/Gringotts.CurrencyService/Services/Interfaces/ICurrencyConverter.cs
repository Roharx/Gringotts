using Gringotts.Shared.Models;
using Gringotts.Shared.Models.CurrencyService;

namespace Gringotts.CurrencyService.Services.Interfaces
{
    public interface ICurrencyConverter
    {
        decimal ConvertToDkk(Money money);
        Money ConvertFromDkk(decimal dkk);
    }
}