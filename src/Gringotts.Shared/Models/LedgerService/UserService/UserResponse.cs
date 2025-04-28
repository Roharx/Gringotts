namespace Gringotts.Shared.Models.LedgerService.UserService
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
    }
}