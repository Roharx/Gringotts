using Gringotts.LedgerService.Data;
using Gringotts.LedgerService.Services.Interfaces;
using Gringotts.Shared.Models.LedgerService.UserService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Gringotts.LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly LedgerDbContext _context;
    private readonly IPasswordHasher _hasher;
    private static readonly ActivitySource ActivitySource = new("LedgerService.UsersController");

    public UsersController(LedgerDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        using var activity = ActivitySource.StartActivity("Register User", ActivityKind.Server);
        activity?.SetTag("user.username", user.Username);

        if (await _context.Users.AnyAsync(u => u.Username == user.Username || u.Email == user.Email))
        {
            return Conflict("Username or email already exists.");
        }

        user.PasswordHash = _hasher.HashPassword(user.PasswordHash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new
        {
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        using var activity = ActivitySource.StartActivity("Login User", ActivityKind.Server);
        activity?.SetTag("user.username", request.Username);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !_hasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            return Unauthorized("Invalid credentials.");
        }

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        using var activity = ActivitySource.StartActivity("Get User By ID", ActivityKind.Server);
        activity?.SetTag("user.id", id.ToString());

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName
        });
    }

    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        using var activity = ActivitySource.StartActivity("Get User By Username", ActivityKind.Server);
        activity?.SetTag("user.username", username);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName
        });
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        using var activity = ActivitySource.StartActivity("Get User By Email", ActivityKind.Server);
        activity?.SetTag("user.email", email);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName
        });
    }
}
