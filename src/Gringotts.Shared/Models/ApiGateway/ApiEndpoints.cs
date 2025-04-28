namespace Gringotts.Shared.Models.ApiGateway;

public static class ApiEndpoints
{
    public const string CurrencyServiceBase = "http://currencyservice:8080/api/currency";
    public const string LedgerServiceBase = "http://ledgerservice:8080/api/transactions";
    public const string LedgerServiceUser = "http://ledgerservice:8080/api/users";
    public const string LedgerServiceRecurring = "http://ledgerservice:8080/api/recurringtransactions";
    public const string LedgerServiceBalance = "http://ledgerservice:8080/api/balances";
}