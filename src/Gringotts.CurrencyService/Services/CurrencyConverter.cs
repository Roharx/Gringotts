using Gringotts.CurrencyService.Services.Interfaces;
using Gringotts.CurrencyService.Services.Models;
using Gringotts.Shared.Models;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService;

namespace Gringotts.CurrencyService.Services
{
    public class CurrencyConverter : ICurrencyConverter
    {
        private readonly CurrencyDbContext _context;

        public CurrencyConverter(CurrencyDbContext context)
        {
            _context = context;
        }

        private ExchangeRate GetLatestRate()
        {
            var rate = _context.ExchangeRates
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefault();

            if (rate == null)
                throw new InvalidOperationException("No exchange rate available.");

            return rate;
        }

        public decimal ConvertToDkk(Money money)
        {
            var rate = GetLatestRate();
            return (money.Galleons * rate.GalleonToDkk) +
                   (money.Sickles * rate.SickleToDkk) +
                   (money.Knuts * rate.KnutToDkk);
        }

        public Money ConvertFromDkk(decimal dkk)
        {
            var rate = GetLatestRate();
            var galleons = (int)(dkk / rate.GalleonToDkk);
            dkk %= rate.GalleonToDkk;

            var sickles = (int)(dkk / rate.SickleToDkk);
            dkk %= rate.SickleToDkk;

            var knuts = (int)(dkk / rate.KnutToDkk);

            return new Money { Galleons = galleons, Sickles = sickles, Knuts = knuts };
        }
    }
}