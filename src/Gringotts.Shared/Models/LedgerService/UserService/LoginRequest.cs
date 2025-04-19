namespace Gringotts.Shared.Models.LedgerService.UserService
{
    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}