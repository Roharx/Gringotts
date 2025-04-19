using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gringotts.LedgerService.Controllers;
using Gringotts.LedgerService.Data;
using Gringotts.LedgerService.Services;
using Gringotts.LedgerService.Services.Interfaces;
using Gringotts.Shared.Models.LedgerService.UserService;

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
        var user = new User
        {
            Username = "wizard1",
            Email = "wizard1@hogwarts.com",
            DisplayName = "Wizard One",
            PasswordHash = "plaintext"
        };

        var result = await controller.Register(user);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("wizard1", ((dynamic)created.Value).Username);
    }

    [Fact]
    public async Task Login_ShouldReturnUserInfo_WhenCredentialsAreValid()
    {
        var context = GetDbContext();
        var hasher = GetHasher();
        var hashedPw = hasher.HashPassword("test123");

        var user = new User
        {
            Username = "validuser",
            Email = "valid@user.com",
            DisplayName = "Valid",
            PasswordHash = hashedPw
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
        Assert.Equal("validuser", ((dynamic)ok.Value).Username);
    }
}
