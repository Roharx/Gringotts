using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService.UserService;

namespace Gringotts.Shared.Models.LedgerService.TransactionService
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
        public TransactionType Type { get; set; }
        public Money Amount { get; set; } = new Money();
        public decimal DkkAmount { get; set; }
        public string? Description { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; }

        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}