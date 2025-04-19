using Xunit;
using Microsoft.AspNetCore.Mvc;
using Gringotts.LedgerService.Controllers;
using Gringotts.LedgerService.Data;
using Gringotts.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService.TransactionService;

namespace Gringotts.LedgerService.Tests.Controllers
{
    public class TransactionsControllerTests
    {
        private LedgerDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LedgerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;
            return new LedgerDbContext(options);
        }

        [Fact]
        public async Task GetTransactions_ReturnsOkWithEmptyList()
        {
            var context = GetInMemoryDbContext();
            var controller = new TransactionsController(context);

            var result = await controller.GetTransactions();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var transactions = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Transaction>>(okResult.Value);
            Assert.Empty(transactions);
        }

        [Fact]
        public async Task AddTransaction_ReturnsCreatedResult()
        {
            var context = GetInMemoryDbContext();
            var controller = new TransactionsController(context);
            var transaction = new Transaction
            {
                Type = TransactionType.Credit,
                DkkAmount = 100,
                Amount = new Money { Galleons = 1, Sickles = 2, Knuts = 3 }
            };

            var result = await controller.AddTransaction(transaction);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var created = Assert.IsType<Transaction>(createdResult.Value);
            Assert.Equal(transaction.DkkAmount, created.DkkAmount);
        }
    }
}