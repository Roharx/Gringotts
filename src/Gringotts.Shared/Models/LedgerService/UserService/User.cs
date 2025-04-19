using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gringotts.Shared.Models.LedgerService.TransactionService;

namespace Gringotts.Shared.Models.LedgerService.UserService
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string DisplayName { get; set; } = null!;

        public ICollection<Transaction>? Transactions { get; set; }
    }
}