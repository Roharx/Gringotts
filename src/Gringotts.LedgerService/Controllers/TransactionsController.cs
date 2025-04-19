using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Data;
using System.Diagnostics;
using Gringotts.Shared.Models;
using Gringotts.Shared.Models.LedgerService.TransactionService;
using Prometheus;

namespace Gringotts.LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly LedgerDbContext _context;
    private static readonly ActivitySource ActivitySource = new("LedgerService.TransactionsController");

    // Prometheus Counters
    private static readonly Counter TransactionsCreated = Metrics.CreateCounter("transactions_created_total", "Total number of transactions successfully created.");
    private static readonly Counter TransactionsFailed = Metrics.CreateCounter("transactions_failed_total", "Total number of failed transaction attempts.");
    private static readonly Counter TransactionsFiltered = Metrics.CreateCounter("transactions_filtered_total", "Total filtered transaction queries.");
    private static readonly Counter TransactionsByIdLookups = Metrics.CreateCounter("transactions_lookup_by_id_total", "Total transaction lookups by ID.");

    public TransactionsController(LedgerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        using var activity = ActivitySource.StartActivity("Get All Transactions", ActivityKind.Server);
        var transactions = await _context.Transactions.ToListAsync();
        activity?.SetTag("transactions.count", transactions.Count);
        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        using var activity = ActivitySource.StartActivity("Get Transaction By Id", ActivityKind.Server);
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction == null)
        {
            return NotFound();
        }

        TransactionsByIdLookups.Inc();
        return Ok(transaction);
    }

    [HttpPost("filter")]
    public async Task<IActionResult> FilterTransactions([FromBody] TransactionFilter filter)
    {
        using var activity = ActivitySource.StartActivity("Filter Transactions", ActivityKind.Server);

        var query = _context.Transactions.AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(t => t.UserId == filter.UserId);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId);

        if (filter.MinAmount.HasValue)
            query = query.Where(t => t.DkkAmount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(t => t.DkkAmount <= filter.MaxAmount.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(t => t.Date >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(t => t.Date <= filter.ToDate.Value);

        var result = await query.ToListAsync();
        activity?.SetTag("transactions.filtered.count", result.Count);
        TransactionsFiltered.Inc();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction([FromBody] Transaction transaction)
    {
        using var activity = ActivitySource.StartActivity("Add Transaction", ActivityKind.Server);

        try
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            TransactionsCreated.Inc();

            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            TransactionsFailed.Inc();
            return StatusCode(500, "Failed to add transaction.");
        }
    }
}
