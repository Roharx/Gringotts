using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Data;
using Gringotts.Shared.Models;
using System.Diagnostics;
using Gringotts.Shared.Models.LedgerService.TransactionService;

namespace Gringotts.LedgerService.Controllers
{
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
        public async Task<IActionResult> GetTransactions()
        {
            using var activity = ActivitySource.StartActivity("Get All Transactions", ActivityKind.Server);
            activity?.SetTag("db.operation", "SELECT");
            activity?.SetTag("db.entity", "Transactions");

            var transactions = await _context.Transactions.ToListAsync();

            activity?.SetTag("transactions.count", transactions.Count);

            return Ok(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] Transaction transaction)
        {
            using var activity = ActivitySource.StartActivity("Add Transaction", ActivityKind.Server);
            activity?.SetTag("db.operation", "INSERT");
            activity?.SetTag("db.entity", "Transactions");
            activity?.SetTag("transaction.id", transaction.Id.ToString());
            activity?.SetTag("transaction.type", transaction.Type.ToString());
            activity?.SetTag("transaction.amount_dkk", transaction.DkkAmount);
            activity?.SetTag("transaction.user_id", transaction.UserId.ToString());

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, transaction);
        }
    }
}