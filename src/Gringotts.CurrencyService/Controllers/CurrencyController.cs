using System.Diagnostics;
using Gringotts.Shared.Models;
using Gringotts.CurrencyService.Services;
using Gringotts.CurrencyService.Services.Interfaces;
using Gringotts.CurrencyService.Services.Models;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace Gringotts.CurrencyService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("CurrencyService.CurrencyController");

    private static readonly Counter ToDkkCounter = Metrics.CreateCounter("currency_converted_to_dkk_total",
        "Total conversions from wizarding currency to DKK.");

    private static readonly Counter FromDkkCounter = Metrics.CreateCounter("currency_converted_from_dkk_total",
        "Total conversions from DKK to wizarding currency.");

    private static readonly Counter ExchangeRateUpdates =
        Metrics.CreateCounter("currency_exchange_rate_updates_total", "Total exchange rate updates.");

    private static readonly Counter FailedExchangeRateUpdates =
        Metrics.CreateCounter("currency_exchange_rate_update_failures_total", "Failed exchange rate updates.");

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
        using var activity = ActivitySource.StartActivity("Convert To DKK", ActivityKind.Server);
        var result = _converter.ConvertToDkk(money);
        activity?.SetTag("currency.dkkAmount", result);
        ToDkkCounter.Inc();
        return Ok(result);
    }

    [HttpPost("convert-from-dkk")]
    public ActionResult<Money> ConvertFromDkk([FromBody] decimal dkk)
    {
        using var activity = ActivitySource.StartActivity("Convert From DKK", ActivityKind.Server);
        var result = _converter.ConvertFromDkk(dkk);
        activity?.SetTag("currency.galleons", result.Galleons);
        FromDkkCounter.Inc();
        return Ok(result);
    }

    [HttpPost("exchange-rate")]
    public async Task<IActionResult> SetExchangeRate([FromBody] ExchangeRate rate)
    {
        if (rate.GalleonToDkk <= 0 || rate.SickleToDkk <= 0 || rate.KnutToDkk <= 0)
        {
            FailedExchangeRateUpdates.Inc();
            return BadRequest("All exchange rates must be greater than zero.");
        }

        rate.EffectiveDate = DateTime.UtcNow;
        _context.ExchangeRates.Add(rate);
        await _context.SaveChangesAsync();
        ExchangeRateUpdates.Inc();

        return Ok(rate);
    }

    [HttpGet("exchange-rate")]
    public ActionResult<ExchangeRate> GetExchangeRate()
    {
        using var activity = ActivitySource.StartActivity("Get Exchange Rate", ActivityKind.Server);
        var rate = _context.ExchangeRates.OrderByDescending(r => r.EffectiveDate).FirstOrDefault();
        if (rate == null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Not found");
            return NotFound();
        }

        activity?.SetTag("exchangeRate.galleonToDkk", rate.GalleonToDkk);
        return Ok(rate);
    }
}