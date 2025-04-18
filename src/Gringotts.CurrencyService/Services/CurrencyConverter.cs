using Gringotts.CurrencyService.Services.Interfaces;
using Gringotts.CurrencyService.Services.Models;
using Gringotts.Shared.Models;

namespace Gringotts.CurrencyService.Services
{
    public class CurrencyConverter : ICurrencyConverter
    {
        private readonly CurrencyDbContext _context;

        public CurrencyConverter(CurrencyDbContext context)
        {
            _context = context;
        }

        public decimal ConvertToDkk(Money money)
        {
            var rate = GetLatestRate();
            var totalGalleons = money.Galleons + money.Sickles / 17m + money.Knuts / 493m;
            return Math.Round(totalGalleons * rate.GalleonToDkk, 2);
        }

        public Money ConvertFromDkk(decimal dkk)
        {
            var rate = GetLatestRate();
            var totalGalleons = dkk / rate.GalleonToDkk;
            var galleons = (int)totalGalleons;
            var remainder = (totalGalleons - galleons) * 17;
            var sickles = (int)remainder;
            var knuts = (int)Math.Round((remainder - sickles) * 29);
            return new Money { Galleons = galleons, Sickles = sickles, Knuts = knuts };
        }

        private ExchangeRate GetLatestRate()
        {
            return _context.ExchangeRates
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefault() ?? new ExchangeRate { GalleonToDkk = 50, EffectiveDate = DateTime.UtcNow };
        }
    }
}