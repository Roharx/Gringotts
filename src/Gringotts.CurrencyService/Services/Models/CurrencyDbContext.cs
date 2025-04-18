using Gringotts.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Gringotts.CurrencyService.Services.Models
{
    public class CurrencyDbContext : DbContext
    {
        public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options) : base(options) { }

        public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    }
}