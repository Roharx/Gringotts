using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Controllers;
using Gringotts.LedgerService.Data;
using Gringotts.LedgerService.Services;
using Gringotts.LedgerService.Services.Interfaces;
using Gringotts.Shared.Models;
using Gringotts.Shared.Models.LedgerService.UserService;
using System.Text.Json;

namespace Gringotts.LedgerService.Tests.Controllers;

public class UsersControllerTests
{
    private LedgerDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<LedgerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LedgerDbContext(options);
    }

    private IPasswordHasher GetHasher() => new PasswordHasher();

    [Fact]
    public async Task Register_ShouldReturnCreatedUser()
    {
        var context = GetDbContext();
        var controller = new UsersController(context, GetHasher());

        var request = new User
        {
            Username = "wizard1",
            Email = "wizard1@hogwarts.com",
            DisplayName = "Wizard One",
            PasswordHash = "plaintext"
        };

        var result = await controller.Register(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var json = JsonSerializer.Serialize(created.Value);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("wizard1", root.GetProperty("Username").GetString());
    }

    [Fact]
    public async Task Login_ShouldReturnUserInfo_WhenCredentialsAreValid()
    {
        var context = GetDbContext();
        var hasher = GetHasher();

        var user = new User
        {
            Username = "validuser",
            Email = "valid@user.com",
            DisplayName = "Valid",
            PasswordHash = hasher.HashPassword("test123")
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new UsersController(context, hasher);
        var result = await controller.Login(new LoginRequest
        {
            Username = "validuser",
            Password = "test123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(ok.Value);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("validuser", root.GetProperty("Username").GetString());
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        var context = GetDbContext();
        var hasher = GetHasher();
        var hashedPw = hasher.HashPassword("correctpass");

        context.Users.Add(new User
        {
            Username = "wrongpwuser",
            Email = "wrong@pw.com",
            DisplayName = "Wrong Password",
            PasswordHash = hashedPw
        });
        await context.SaveChangesAsync();

        var controller = new UsersController(context, hasher);

        var result = await controller.Login(new LoginRequest
        {
            Username = "wrongpwuser",
            Password = "wrongpass"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetByUsername_ShouldReturnCorrectUser()
    {
        var context = GetDbContext();
        var user = new User
        {
            Username = "seeker",
            Email = "seeker@hogwarts.com",
            DisplayName = "Golden Snitch",
            PasswordHash = "irrelevant"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new UsersController(context, GetHasher());

        var result = await controller.GetByUsername("seeker");
        var ok = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(ok.Value);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("seeker", root.GetProperty("Username").GetString());
    }
}
