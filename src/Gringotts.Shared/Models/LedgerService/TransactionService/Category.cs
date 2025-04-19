using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gringotts.Shared.Models.LedgerService.TransactionService
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}