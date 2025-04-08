using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gringotts.Shared.Models
{
    public enum RecurrenceFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    public class RecurringTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        public string Description { get; set; } = null!;
        public decimal DkkAmount { get; set; }
        public int Galleons { get; set; }
        public int Sickles { get; set; }
        public int Knuts { get; set; }
        public RecurrenceFrequency Frequency { get; set; }
        public DateTime NextOccurrence { get; set; }
    }
}