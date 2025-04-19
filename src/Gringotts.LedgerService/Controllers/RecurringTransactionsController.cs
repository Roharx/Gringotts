using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Gringotts.LedgerService.Data;
using Gringotts.Shared.Models.LedgerService.TransactionService;

namespace Gringotts.LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecurringTransactionsController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("LedgerService.RecurringTransactionsController");

    private static readonly Counter RecurringCreates = Metrics.CreateCounter("recurring_transactions_created_total", "Recurring transactions created.");
    private static readonly Counter RecurringUpdates = Metrics.CreateCounter("recurring_transactions_updated_total", "Recurring transactions updated.");
    private static readonly Counter RecurringDeletes = Metrics.CreateCounter("recurring_transactions_deleted_total", "Recurring transactions deleted.");
    private static readonly Counter RecurringFailedUpdates = Metrics.CreateCounter("recurring_transactions_failed_updates_total", "Recurring transactions that failed to update.");
    private static readonly Counter RecurringFailedCreates = Metrics.CreateCounter("recurring_transactions_failed_creates_total", "Recurring transactions that failed to be created.");
    private static readonly Counter RecurringFailedDeletes = Metrics.CreateCounter("recurring_transactions_failed_deletes_total", "Recurring transactions that failed to be deleted.");

    private readonly LedgerDbContext _context;

    public RecurringTransactionsController(LedgerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<RecurringTransaction>>> GetAll()
    {
        using var activity = ActivitySource.StartActivity("Get All Recurring Transactions", ActivityKind.Server);
        var list = await _context.RecurringTransactions.ToListAsync();
        activity?.SetTag("recurring.count", list.Count);
        return list;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecurringTransaction>> GetById(Guid id)
    {
        using var activity = ActivitySource.StartActivity("Get Recurring Transaction By Id", ActivityKind.Server);
        var rt = await _context.RecurringTransactions.FindAsync(id);
        if (rt == null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Not found");
            return NotFound();
        }

        return Ok(rt);
    }

    [HttpPost]
    public async Task<ActionResult<RecurringTransaction>> Create(RecurringTransaction rt)
    {
        using var activity = ActivitySource.StartActivity("Create Recurring Transaction", ActivityKind.Server);
        try
        {
            rt.NextOccurrence = rt.NextOccurrence.ToUniversalTime();
            _context.RecurringTransactions.Add(rt);
            await _context.SaveChangesAsync();
            RecurringCreates.Inc();

            return CreatedAtAction(nameof(GetById), new { id = rt.Id }, rt);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            RecurringFailedCreates.Inc();
            return StatusCode(500, "Failed to create recurring transaction.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, RecurringTransaction rt)
    {
        using var activity = ActivitySource.StartActivity("Update Recurring Transaction", ActivityKind.Server);

        if (id != rt.Id)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "ID mismatch");
            RecurringFailedUpdates.Inc();
            return BadRequest("ID mismatch.");
        }

        var existing = await _context.RecurringTransactions.FindAsync(id);
        if (existing == null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Not found");
            RecurringFailedUpdates.Inc();
            return NotFound();
        }

        existing.Description = rt.Description;
        existing.DkkAmount = rt.DkkAmount;
        existing.Galleons = rt.Galleons;
        existing.Sickles = rt.Sickles;
        existing.Knuts = rt.Knuts;
        existing.Frequency = rt.Frequency;
        existing.NextOccurrence = rt.NextOccurrence.ToUniversalTime();

        await _context.SaveChangesAsync();
        RecurringUpdates.Inc();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        using var activity = ActivitySource.StartActivity("Delete Recurring Transaction", ActivityKind.Server);

        var existing = await _context.RecurringTransactions.FindAsync(id);
        if (existing == null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Not found");
            RecurringFailedDeletes.Inc();
            return NotFound();
        }

        _context.RecurringTransactions.Remove(existing);
        await _context.SaveChangesAsync();
        RecurringDeletes.Inc();
        return NoContent();
    }
}
