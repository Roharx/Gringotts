using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Data;
using System.Diagnostics;
using Gringotts.Shared.Models.CurrencyService;
using Prometheus;

namespace Gringotts.LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BalancesController : ControllerBase
{
    private readonly LedgerDbContext _context;
    private static readonly ActivitySource ActivitySource = new("LedgerService.BalancesController");

    private static readonly Counter BalancesFetched =
        Metrics.CreateCounter("balances_fetched_total", "Total number of balance fetches");

    public BalancesController(LedgerDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetBalance(Guid userId)
    {
        using var activity = ActivitySource.StartActivity("GetBalance", ActivityKind.Server);

        var balance = await _context.Balances.FirstOrDefaultAsync(b => b.UserId == userId);

        if (balance == null)
        {
            return NotFound();
        }

        BalancesFetched.Inc();
        return Ok(balance);
    }
    
    [HttpPost("add")]
    public async Task<IActionResult> AddBalance([FromBody] BalanceUpdateRequest request)
    {
        using var activity = ActivitySource.StartActivity("AddBalance", ActivityKind.Server);

        var balance = await _context.Balances
            .FirstOrDefaultAsync(b => b.UserId == request.UserId);

        if (balance == null)
        {
            balance = new Balance
            {
                UserId    = request.UserId,
                DkkAmount = request.DkkAmount,
                Galleons  = request.Galleons,
                Sickles   = request.Sickles,
                Knuts     = request.Knuts,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Balances.Add(balance);
        }
        else
        {
            balance.DkkAmount += request.DkkAmount;
            balance.Galleons  += request.Galleons;
            balance.Sickles   += request.Sickles;
            balance.Knuts     += request.Knuts;
            balance.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        BalancesFetched.Inc();
        return Ok(balance);
    }
}