using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Data;
using Gringotts.Shared.Models;

namespace Gringotts.LedgerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly LedgerDbContext _context;

        public TransactionsController(LedgerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions()
        {
            var transactions = await _context.Transactions.ToListAsync();
            return Ok(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, transaction);
        }
    }
}