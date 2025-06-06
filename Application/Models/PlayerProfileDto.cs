namespace Application.Models
{
    public class PlayerProfileDTO
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<WalletDTO> Wallets { get; set; } = new();
    }
}
