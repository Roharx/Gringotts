using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Data;
using System.Diagnostics;
using Gringotts.Shared.Models.LedgerService.TransactionService;

namespace Gringotts.LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly LedgerDbContext _context;
    private static readonly ActivitySource ActivitySource = new("LedgerService.TransactionsController");

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
        activity?.SetTag("transaction.id", id.ToString());

        var transaction = await _context.Transactions.FindAsync(id);
        return transaction == null ? NotFound() : Ok(transaction);
    }

    [HttpPost("filter")]
    public async Task<IActionResult> FilterTransactions([FromBody] TransactionFilter filter)
    {
        using var activity = ActivitySource.StartActivity("Filter Transactions", ActivityKind.Server);

        var query = _context.Transactions.AsQueryable();

        if (filter.UserId.HasValue)
        {
            activity?.SetTag("filter.user_id", filter.UserId.ToString());
            query = query.Where(t => t.UserId == filter.UserId);
        }

        if (filter.CategoryId.HasValue)
        {
            activity?.SetTag("filter.category_id", filter.CategoryId.ToString());
            query = query.Where(t => t.CategoryId == filter.CategoryId);
        }

        if (filter.MinAmount.HasValue)
        {
            activity?.SetTag("filter.min_amount", filter.MinAmount.ToString());
            query = query.Where(t => t.DkkAmount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            activity?.SetTag("filter.max_amount", filter.MaxAmount.ToString());
            query = query.Where(t => t.DkkAmount <= filter.MaxAmount.Value);
        }

        if (filter.FromDate.HasValue)
        {
            activity?.SetTag("filter.from_date", filter.FromDate.ToString());
            query = query.Where(t => t.Date >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            activity?.SetTag("filter.to_date", filter.ToDate.ToString());
            query = query.Where(t => t.Date <= filter.ToDate.Value);
        }

        var result = await query.ToListAsync();
        activity?.SetTag("transactions.filtered.count", result.Count);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction([FromBody] Transaction transaction)
    {
        using var activity = ActivitySource.StartActivity("Add Transaction", ActivityKind.Server);
        activity?.SetTag("transaction.user_id", transaction.UserId?.ToString());
        activity?.SetTag("transaction.amount_dkk", transaction.DkkAmount);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
    }
}
