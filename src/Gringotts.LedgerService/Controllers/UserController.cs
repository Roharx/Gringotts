using Prometheus;
using Gringotts.LedgerService.Data;
using Gringotts.LedgerService.Services.Interfaces;
using Gringotts.Shared.Models;
using Gringotts.Shared.Models.LedgerService.UserService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gringotts.LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly Counter Registrations = Metrics.CreateCounter("users_registered_total", "Total users registered.");
    private static readonly Counter LoginAttempts = Metrics.CreateCounter("users_login_attempts_total", "Total login attempts.");
    private static readonly Counter FailedLogins = Metrics.CreateCounter("users_failed_logins_total", "Total failed login attempts.");
    private static readonly Counter UserRetrievals = Metrics.CreateCounter("users_retrieved_total", "Total users retrieved.");

    private readonly LedgerDbContext _context;
    private readonly IPasswordHasher _hasher;

    public UsersController(LedgerDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        if (await _context.Users.AnyAsync(u => u.Username == user.Username || u.Email == user.Email))
            return Conflict("Username or email already exists.");

        user.PasswordHash = _hasher.HashPassword(user.PasswordHash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        Registrations.Inc();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { user.Id, user.Username, user.Email, user.DisplayName });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        LoginAttempts.Inc();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !_hasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            FailedLogins.Inc();
            return Unauthorized("Invalid credentials.");
        }

        return Ok(new { user.Id, user.Username, user.Email, user.DisplayName });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        UserRetrievals.Inc();
        return Ok(new { user.Id, user.Username, user.Email, user.DisplayName });
    }

    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return NotFound();
        UserRetrievals.Inc();
        return Ok(new { user.Id, user.Username, user.Email, user.DisplayName });
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();
        UserRetrievals.Inc();
        return Ok(new { user.Id, user.Username, user.Email, user.DisplayName });
    }
}
