namespace Gringotts.Shared.Models.LedgerService.TransactionService;

public class TransactionFilter
{
    public Guid? UserId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}