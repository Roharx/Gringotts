namespace Gringotts.Shared.Models.ApiGateway;

public static class FeatureToggles
{
    public static bool IsLoginEnabled { get; set; } = true;
    public static bool IsRegisterEnabled { get; set; } = true;
    public static bool IsTransactionEnabled { get; set; } = true;
    public static bool IsConversionEnabled { get; set; } = true;
    public static bool IsRecurringEnabled { get; set; } = true;
    public static bool IsExchangeRateEnabled { get; set; } = true;
}