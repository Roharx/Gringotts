using System.Diagnostics;
using Gringotts.Shared.Models.ApiGateway;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService;
using Gringotts.Shared.Models.LedgerService.TransactionService;
using Gringotts.Shared.Models.LedgerService.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace Gringotts.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GatewayController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("ApiGateway.GatewayController");

    #region Prometheus

    #region Convert

    private static readonly Counter ConvertToDkkSuccess =
        Metrics.CreateCounter("gateway_convert_to_dkk_success_total", "Successful ConvertToDkk calls");
    private static readonly Counter ConvertToDkkFailed =
        Metrics.CreateCounter("gateway_convert_to_dkk_failed_total", "Failed ConvertToDkk calls");
    private static readonly Counter ConvertFromDkkSuccess =
        Metrics.CreateCounter("gateway_convert_from_dkk_success_total", "Successful ConvertFromDkk calls");
    private static readonly Counter ConvertFromDkkFailed =
        Metrics.CreateCounter("gateway_convert_from_dkk_failed_total", "Failed ConvertFromDkk calls");

    #endregion

    #region Exchange

    private static readonly Counter ExchangeRateGets =
        Metrics.CreateCounter("gateway_exchange_rate_gets_total", "Exchange rate fetches");
    private static readonly Counter ExchangeRateSets =
        Metrics.CreateCounter("gateway_exchange_rate_sets_total", "Exchange rate updates");
    private static readonly Counter ExchangeRateSetFails =
        Metrics.CreateCounter("gateway_exchange_rate_set_fails_total", "Failed exchange rate updates");

    #endregion

    #region Transaction

    private static readonly Counter TransactionsFetched =
        Metrics.CreateCounter("gateway_transactions_fetched_total", "Total transactions fetched");
    private static readonly Counter TransactionsFetchFailed =
        Metrics.CreateCounter("gateway_transactions_fetch_failed_total", "Failed transaction fetch attempts");
    private static readonly Counter TransactionsPosted =
        Metrics.CreateCounter("gateway_transactions_posted_total", "Total transactions posted");
    private static readonly Counter TransactionsPostFailed =
        Metrics.CreateCounter("gateway_transactions_post_failed_total", "Failed transaction post attempts");

    #endregion

    #region User

    private static readonly Counter UsersRegistered =
        Metrics.CreateCounter("gateway_users_registered_total", "Total user registrations");
    private static readonly Counter UsersRegisterFailed =
        Metrics.CreateCounter("gateway_users_register_failed_total", "Failed user registrations");
    private static readonly Counter UsersLoggedIn =
        Metrics.CreateCounter("gateway_users_logged_in_total", "Total successful logins");
    private static readonly Counter UsersLoginFailed =
        Metrics.CreateCounter("gateway_users_login_failed_total", "Failed login attempts");
    private static readonly Counter UsersFetched =
        Metrics.CreateCounter("gateway_users_fetched_total", "User retrievals");
    private static readonly Counter UsersFetchFailed =
        Metrics.CreateCounter("gateway_users_fetch_failed_total", "Failed user retrievals");

    #endregion

    #region Recurring

    private static readonly Counter RecurringFetched =
        Metrics.CreateCounter("gateway_recurring_fetched_total", "Total recurring transactions fetched");
    private static readonly Counter RecurringFetchFailed =
        Metrics.CreateCounter("gateway_recurring_fetch_failed_total", "Failed recurring transaction fetch attempts");
    private static readonly Counter RecurringPosted =
        Metrics.CreateCounter("gateway_recurring_posted_total", "Total recurring transactions posted");
    private static readonly Counter RecurringPostFailed =
        Metrics.CreateCounter("gateway_recurring_post_failed_total", "Failed recurring transaction post attempts");

    #endregion

    #region Balance

    private static readonly Counter BalancesFetched =
        Metrics.CreateCounter("gateway_balances_fetched_total", "Total balance fetches");
    private static readonly Counter BalancesFetchFailed =
        Metrics.CreateCounter("gateway_balances_fetch_failed_total", "Failed balance fetch attempts");

    #endregion
    
    #endregion

    private readonly HttpClient _http;

    public GatewayController(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
    }
    [AllowAnonymous]
    [HttpPost("users/register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
    {
        using var activity = ActivitySource.StartActivity("RegisterUser", ActivityKind.Server);

        // Create the user
        var response = await _http.PostAsJsonAsync($"{ApiEndpoints.LedgerServiceUser}/register", request);
        if (!response.IsSuccessStatusCode)
        {
            UsersRegisterFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Registration failed");
            return StatusCode((int)response.StatusCode);
        }

        var createdUser = await response.Content.ReadFromJsonAsync<User>();
        UsersRegistered.Inc();

        // Create the user's starting balance
        var createBalanceRequest = new
        {
            userId = createdUser.Id,
            dkkAmount = 0,
            galleons = 0,
            sickles = 0,
            knuts = 0
        };

        var balanceResponse = await _http.PostAsJsonAsync($"{ApiEndpoints.LedgerServiceBalance}/add", createBalanceRequest);
        if (!balanceResponse.IsSuccessStatusCode)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to create initial balance");
            return StatusCode((int)balanceResponse.StatusCode);
        }

        // Return the user info
        return Ok(createdUser);
    }

    [AllowAnonymous]
    [HttpPost("users/login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginRequest request)
    {
        using var activity = ActivitySource.StartActivity("LoginUser", ActivityKind.Server);
        var response = await _http.PostAsJsonAsync($"{ApiEndpoints.LedgerServiceUser}/login", request);
        if (!response.IsSuccessStatusCode)
        {
            UsersLoginFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Login failed");
            return StatusCode((int)response.StatusCode);
        }

        UsersLoggedIn.Inc();
        return Ok(await response.Content.ReadFromJsonAsync<object>());
    }

    [HttpGet("users/by-id/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        using var activity = ActivitySource.StartActivity("GetUserById", ActivityKind.Server);
        var response = await _http.GetAsync($"{ApiEndpoints.LedgerServiceUser}/{id}");
        if (!response.IsSuccessStatusCode)
        {
            UsersFetchFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "User by ID fetch failed");
            return StatusCode((int)response.StatusCode);
        }

        UsersFetched.Inc();
        return Ok(await response.Content.ReadFromJsonAsync<object>());
    }

    [HttpGet("users/by-username/{username}")]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        using var activity = ActivitySource.StartActivity("GetUserByUsername", ActivityKind.Server);
        var response = await _http.GetAsync($"{ApiEndpoints.LedgerServiceUser}/by-username/{username}");
        if (!response.IsSuccessStatusCode)
        {
            UsersFetchFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "User by username fetch failed");
            return StatusCode((int)response.StatusCode);
        }

        UsersFetched.Inc();
        return Ok(await response.Content.ReadFromJsonAsync<object>());
    }

    [HttpGet("users/by-email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        using var activity = ActivitySource.StartActivity("GetUserByEmail", ActivityKind.Server);
        var response = await _http.GetAsync($"{ApiEndpoints.LedgerServiceUser}/by-email/{email}");
        if (!response.IsSuccessStatusCode)
        {
            UsersFetchFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "User by email fetch failed");
            return StatusCode((int)response.StatusCode);
        }

        UsersFetched.Inc();
        return Ok(await response.Content.ReadFromJsonAsync<object>());
    }

    [HttpPost("convert-to-dkk")]
    public async Task<ActionResult<decimal>> ConvertToDkk([FromBody] Money money)
    {
        using var activity = ActivitySource.StartActivity("ConvertToDkk", ActivityKind.Server);
        var response = await _http.PostAsJsonAsync($"{ApiEndpoints.CurrencyServiceBase}/convert-to-dkk", money);
        if (!response.IsSuccessStatusCode)
        {
            ConvertToDkkFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to convert to DKK");
            return StatusCode((int)response.StatusCode);
        }

        ConvertToDkkSuccess.Inc();
        var result = await response.Content.ReadFromJsonAsync<decimal>();
        activity?.SetTag("converted.dkkAmount", result);
        return result!;
    }

    [HttpPost("convert-from-dkk")]
    public async Task<ActionResult<Money>> ConvertFromDkk([FromBody] decimal dkk)
    {
        using var activity = ActivitySource.StartActivity("ConvertFromDkk", ActivityKind.Server);
        var response = await _http.PostAsJsonAsync($"{ApiEndpoints.CurrencyServiceBase}/convert-from-dkk", dkk);
        if (!response.IsSuccessStatusCode)
        {
            ConvertFromDkkFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to convert from DKK");
            return StatusCode((int)response.StatusCode);
        }

        ConvertFromDkkSuccess.Inc();
        var result = await response.Content.ReadFromJsonAsync<Money>();
        activity?.SetTag("converted.galleons", result?.Galleons ?? 0);
        return result!;
    }

    [HttpGet("exchange-rate")]
    public async Task<ActionResult<ExchangeRate>> GetExchangeRate()
    {
        using var activity = ActivitySource.StartActivity("GetExchangeRate", ActivityKind.Server);
        var response = await _http.GetAsync($"{ApiEndpoints.CurrencyServiceBase}/exchange-rate");
        if (!response.IsSuccessStatusCode)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to get exchange rate");
            return StatusCode((int)response.StatusCode);
        }

        ExchangeRateGets.Inc();
        var result = await response.Content.ReadFromJsonAsync<ExchangeRate>();
        activity?.SetTag("rate.galleonToDkk", result?.GalleonToDkk ?? 0);
        return result!;
    }

    [HttpPost("exchange-rate")]
    public async Task<ActionResult<ExchangeRate>> SetExchangeRate([FromBody] ExchangeRate rate)
    {
        using var activity = ActivitySource.StartActivity("SetExchangeRate", ActivityKind.Server);
        var response = await _http.PostAsJsonAsync($"{ApiEndpoints.CurrencyServiceBase}/exchange-rate", rate);
        if (!response.IsSuccessStatusCode)
        {
            ExchangeRateSetFails.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to set exchange rate");
            return StatusCode((int)response.StatusCode);
        }

        ExchangeRateSets.Inc();
        return await response.Content.ReadFromJsonAsync<ExchangeRate>()!;
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<List<Transaction>>> GetTransactions()
    {
        using var activity = ActivitySource.StartActivity("GetTransactions", ActivityKind.Server);
        var response = await _http.GetAsync(ApiEndpoints.LedgerServiceBase);
        if (!response.IsSuccessStatusCode)
        {
            TransactionsFetchFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to get transactions");
            return StatusCode((int)response.StatusCode);
        }

        TransactionsFetched.Inc();
        var result = await response.Content.ReadFromJsonAsync<List<Transaction>>();
        activity?.SetTag("transactions.count", result?.Count ?? 0);
        return result!;
    }

    [HttpPost("transactions")]
    public async Task<ActionResult<Transaction>> AddTransaction([FromBody] Transaction transaction)
    {
        using var activity = ActivitySource.StartActivity("AddTransaction", ActivityKind.Server);

        // Record the transaction
        var transactionResponse = await _http.PostAsJsonAsync(ApiEndpoints.LedgerServiceBase, transaction);
        if (!transactionResponse.IsSuccessStatusCode)
        {
            TransactionsPostFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to post transaction");
            return StatusCode((int)transactionResponse.StatusCode);
        }

        var createdTransaction = await transactionResponse.Content.ReadFromJsonAsync<Transaction>();
        TransactionsPosted.Inc();
        activity?.SetTag("transaction.amountDkk", createdTransaction?.DkkAmount ?? 0);

        // Update the balance
        var balanceUpdate = new
        {
            userId = transaction.UserId,
            dkkAmount = transaction.DkkAmount,
            galleons = transaction.Amount.Galleons,
            sickles = transaction.Amount.Sickles,
            knuts = transaction.Amount.Knuts
        };

        var balanceResponse = await _http.PostAsJsonAsync($"{ApiEndpoints.LedgerServiceBalance}/add", balanceUpdate);
        if (!balanceResponse.IsSuccessStatusCode)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to update balance after transaction");
        }

        return Ok(createdTransaction);
    }


    [HttpGet("recurring-transactions")]
    public async Task<ActionResult<List<RecurringTransaction>>> GetRecurringTransactions()
    {
        using var activity = ActivitySource.StartActivity("GetRecurringTransactions", ActivityKind.Server);
        var response = await _http.GetAsync(ApiEndpoints.LedgerServiceRecurring);
        if (!response.IsSuccessStatusCode)
        {
            RecurringFetchFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to fetch recurring transactions");
            return StatusCode((int)response.StatusCode);
        }

        RecurringFetched.Inc();
        var result = await response.Content.ReadFromJsonAsync<List<RecurringTransaction>>();
        activity?.SetTag("recurring.count", result?.Count ?? 0);
        return result!;
    }

    [HttpGet("recurring-transactions/{id:guid}")]
    public async Task<ActionResult<RecurringTransaction>> GetRecurringTransactionById(Guid id)
    {
        using var activity = ActivitySource.StartActivity("GetRecurringTransactionById", ActivityKind.Server);
        var response = await _http.GetAsync($"{ApiEndpoints.LedgerServiceRecurring}/{id}");
        if (!response.IsSuccessStatusCode)
        {
            RecurringFetchFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to fetch recurring transaction by ID");
            return StatusCode((int)response.StatusCode);
        }

        RecurringFetched.Inc();
        return await response.Content.ReadFromJsonAsync<RecurringTransaction>()!;
    }

    [HttpPost("recurring-transactions")]
    public async Task<ActionResult<RecurringTransaction>> AddRecurringTransaction(
        [FromBody] RecurringTransaction transaction)
    {
        using var activity = ActivitySource.StartActivity("AddRecurringTransaction", ActivityKind.Server);
        var response = await _http.PostAsJsonAsync(ApiEndpoints.LedgerServiceRecurring, transaction);
        if (!response.IsSuccessStatusCode)
        {
            RecurringPostFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to add recurring transaction");
            return StatusCode((int)response.StatusCode);
        }

        RecurringPosted.Inc();
        var result = await response.Content.ReadFromJsonAsync<RecurringTransaction>();
        activity?.SetTag("recurring.amountDkk", result?.DkkAmount ?? 0);
        return result!;
    }
    
    [HttpGet("balance/{userId:guid}")]
    public async Task<IActionResult> GetBalance(Guid userId)
    {
        using var activity = ActivitySource.StartActivity("GetBalance", ActivityKind.Server);
        var response = await _http.GetAsync($"{ApiEndpoints.LedgerServiceBalance}/{userId}");

        if (!response.IsSuccessStatusCode)
        {
            BalancesFetchFailed.Inc();
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to fetch balance");
            return StatusCode((int)response.StatusCode);
        }

        BalancesFetched.Inc();
        var result = await response.Content.ReadFromJsonAsync<object>();
        activity?.SetTag("balance.userId", userId);
        return Ok(result);
    }
}