using Gringotts.LedgerService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Gringotts.Shared.Models.LedgerService.TransactionService;

namespace Gringotts.LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecurringTransactionsController : ControllerBase
{
    private readonly LedgerDbContext _context;
    private static readonly ActivitySource ActivitySource = new("LedgerService.RecurringTransactionsController");

    public RecurringTransactionsController(LedgerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<RecurringTransaction>>> GetAll()
    {
        using var activity = ActivitySource.StartActivity("Get All Recurring Transactions", ActivityKind.Server);
        var result = await _context.RecurringTransactions.ToListAsync();
        activity?.SetTag("recurring.count", result.Count);
        return result;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecurringTransaction>> GetById(Guid id)
    {
        using var activity = ActivitySource.StartActivity("Get Recurring Transaction By Id", ActivityKind.Server);
        activity?.SetTag("recurring.id", id.ToString());

        var rt = await _context.RecurringTransactions.FindAsync(id);
        return rt is null ? NotFound() : Ok(rt);
    }

    [HttpPost]
    public async Task<ActionResult<RecurringTransaction>> Create(RecurringTransaction rt)
    {
        using var activity = ActivitySource.StartActivity("Create Recurring Transaction", ActivityKind.Server);
        activity?.SetTag("recurring.amount_dkk", rt.DkkAmount);
        activity?.SetTag("recurring.frequency", rt.Frequency.ToString());

        rt.NextOccurrence = rt.NextOccurrence.ToUniversalTime();

        _context.RecurringTransactions.Add(rt);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = rt.Id }, rt);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, RecurringTransaction rt)
    {
        using var activity = ActivitySource.StartActivity("Update Recurring Transaction", ActivityKind.Server);
        activity?.SetTag("recurring.id", id.ToString());

        if (id != rt.Id)
            return BadRequest("ID mismatch.");

        var existing = await _context.RecurringTransactions.FindAsync(id);
        if (existing is null)
            return NotFound();

        existing.Description = rt.Description;
        existing.DkkAmount = rt.DkkAmount;
        existing.Galleons = rt.Galleons;
        existing.Sickles = rt.Sickles;
        existing.Knuts = rt.Knuts;
        existing.Frequency = rt.Frequency;
        existing.NextOccurrence = rt.NextOccurrence.ToUniversalTime();

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        using var activity = ActivitySource.StartActivity("Delete Recurring Transaction", ActivityKind.Server);
        activity?.SetTag("recurring.id", id.ToString());

        var existing = await _context.RecurringTransactions.FindAsync(id);
        if (existing is null)
            return NotFound();

        _context.RecurringTransactions.Remove(existing);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
