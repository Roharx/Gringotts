using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Controllers;
using Gringotts.LedgerService.Data;
using Gringotts.Shared.Models.CurrencyService;
using Gringotts.Shared.Models.LedgerService.TransactionService;

namespace Gringotts.LedgerService.Tests.Controllers;

public class TransactionsControllerTests
{
    private LedgerDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<LedgerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LedgerDbContext(options);
    }

    [Fact]
    public async Task AddTransaction_ShouldCreateAndReturnTransaction()
    {
        var context = GetDbContext();
        var controller = new TransactionsController(context);
        var transaction = new Transaction
        {
            Type = TransactionType.Debit,
            DkkAmount = 200,
            Amount = new Money { Galleons = 1 }
        };

        var result = await controller.AddTransaction(transaction);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var returned = Assert.IsType<Transaction>(created.Value);
        Assert.Equal(200, returned.DkkAmount);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyListInitially()
    {
        var context = GetDbContext();
        var controller = new TransactionsController(context);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<List<Transaction>>(ok.Value);
        Assert.Empty(list);
    }
}