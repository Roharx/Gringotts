using Gringotts.Shared.Models;
using Gringotts.Shared.Models.LedgerService.TransactionService;
using Gringotts.TransactionPublisher.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gringotts.TransactionPublisher.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionMessageProducer _producer;
    public TransactionsController(TransactionMessageProducer producer)
    {
        _producer = producer;
    }

    [HttpPost]
    public IActionResult PostTransaction([FromBody] Transaction transaction)
    {
        _producer.PublishTransaction(transaction);
        return Accepted();
    }
}