namespace Application.Models
{
    public class WalletDTO
    {
        public Guid Id { get; set; }
        public decimal Balance { get; set; }
        public int CurrencyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public CurrencyDTO? Currency { get; set; }
    }
}
