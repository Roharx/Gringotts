using Gringotts.Shared.Models.LedgerService.UserService;

namespace Gringotts.LedgerService.Services.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}