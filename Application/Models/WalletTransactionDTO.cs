namespace Application.Models
{
    public class WalletTransactionDTO
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
