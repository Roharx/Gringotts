using Gringotts.Shared.Models;
using Gringotts.Shared.Models.ApiGateway;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService;
using Gringotts.Shared.Models.LedgerService.TransactionService;
using Microsoft.AspNetCore.Mvc;

namespace Gringotts.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly HttpClient _http;

    public GatewayController(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
    }

    // Currency Endpoints

    [HttpPost("convert-to-dkk")]
    public async Task<ActionResult<decimal>> ConvertToDkk([FromBody] Money money)
    {
        var response = await _http.PostAsJsonAsync($"{ApiEndpoints.CurrencyServiceBase}/convert-to-dkk", money);
        if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
        return await response.Content.ReadFromJsonAsync<decimal>();
    }

    [HttpPost("convert-from-dkk")]
    public async Task<ActionResult<Money>> ConvertFromDkk([FromBody] decimal dkk)
    {
        var response = await _http.PostAsJsonAsync($"{ApiEndpoints.CurrencyServiceBase}/convert-from-dkk", dkk);
        if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Money>();
    }

    [HttpGet("exchange-rate")]
    public async Task<ActionResult<ExchangeRate>> GetExchangeRate()
    {
        var response = await _http.GetAsync($"{ApiEndpoints.CurrencyServiceBase}/exchange-rate");
        if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
        return await response.Content.ReadFromJsonAsync<ExchangeRate>();
    }

    [HttpPost("exchange-rate")]
    public async Task<ActionResult<ExchangeRate>> SetExchangeRate([FromBody] ExchangeRate rate)
    {
        var response = await _http.PostAsJsonAsync($"{ApiEndpoints.CurrencyServiceBase}/exchange-rate", rate);
        if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
        return await response.Content.ReadFromJsonAsync<ExchangeRate>();
    }

    // Transaction Endpoints

    [HttpGet("transactions")]
    public async Task<ActionResult<List<Transaction>>> GetTransactions()
    {
        var response = await _http.GetAsync(ApiEndpoints.LedgerServiceBase);
        if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
        return await response.Content.ReadFromJsonAsync<List<Transaction>>();
    }

    [HttpPost("transactions")]
    public async Task<ActionResult<Transaction>> AddTransaction([FromBody] Transaction transaction)
    {
        var response = await _http.PostAsJsonAsync(ApiEndpoints.LedgerServiceBase, transaction);
        if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Transaction>();
    }
}
