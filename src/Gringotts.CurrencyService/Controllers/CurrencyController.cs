using Gringotts.Shared.Models;
using Gringotts.CurrencyService.Services;
using Gringotts.CurrencyService.Services.Interfaces;
using Gringotts.CurrencyService.Services.Models;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService;
using Microsoft.AspNetCore.Mvc;

namespace Gringotts.CurrencyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyConverter _converter;
        private readonly CurrencyDbContext _context;

        public CurrencyController(ICurrencyConverter converter, CurrencyDbContext context)
        {
            _converter = converter;
            _context = context;
        }

        [HttpPost("convert-to-dkk")]
        public ActionResult<decimal> ConvertToDkk([FromBody] Money money)
        {
            return Ok(_converter.ConvertToDkk(money));
        }

        [HttpPost("convert-from-dkk")]
        public ActionResult<Money> ConvertFromDkk([FromBody] decimal dkk)
        {
            return Ok(_converter.ConvertFromDkk(dkk));
        }

        [HttpPost("exchange-rate")]
        public async Task<IActionResult> SetExchangeRate([FromBody] ExchangeRate rate)
        {
            rate.EffectiveDate = DateTime.UtcNow;
            _context.ExchangeRates.Add(rate);
            await _context.SaveChangesAsync();
            return Ok(rate);
        }

        [HttpGet("exchange-rate")]
        public ActionResult<ExchangeRate> GetExchangeRate()
        {
            var rate = _context.ExchangeRates
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefault();
            return rate == null ? NotFound() : Ok(rate);
        }
    }
}