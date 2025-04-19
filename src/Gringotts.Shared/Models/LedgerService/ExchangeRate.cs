using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gringotts.Shared.Models.LedgerService
{
    public class ExchangeRate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        public decimal GalleonToDkk { get; set; }
        public decimal SickleToDkk { get; set; }
        public decimal KnutToDkk { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}