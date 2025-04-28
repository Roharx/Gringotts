using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gringotts.Shared.Models.CurrencyService
{
    public class Balance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [Column(TypeName = "numeric(18,2)")]
        public decimal DkkAmount { get; set; } = 0m;

        [Required]
        public int Galleons { get; set; } = 0;

        [Required]
        public int Sickles { get; set; } = 0;

        [Required]
        public int Knuts { get; set; } = 0;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}