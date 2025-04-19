using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Controllers;
using Gringotts.LedgerService.Data;
using Gringotts.Shared.Models.LedgerService.TransactionService;

namespace Gringotts.LedgerService.Tests.Controllers;

public class RecurringTransactionsControllerTests
{
    private LedgerDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<LedgerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LedgerDbContext(options);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated()
    {
        var context = GetDbContext();
        var controller = new RecurringTransactionsController(context);
        var rt = new RecurringTransaction
        {
            Description = "Rent",
            DkkAmount = 1000,
            Galleons = 2,
            Sickles = 0,
            Knuts = 0,
            Frequency = RecurrenceFrequency.Monthly,
            NextOccurrence = DateTime.UtcNow.AddDays(30)
        };

        var result = await controller.Create(rt);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<RecurringTransaction>(created.Value);
        Assert.Equal("Rent", returned.Description);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyInitially()
    {
        var context = GetDbContext();
        var controller = new RecurringTransactionsController(context);

        var result = await controller.GetAll();
        Assert.Empty(result.Value);
    }
}