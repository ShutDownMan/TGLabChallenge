namespace Application.Models
{
    public record PlaceBetDTO
    {
        public Guid WalletId { get; init; }
        public Guid GameId { get; init; }
        public decimal Amount { get; init; }
        public int CurrencyId { get; init; }
        public DateTime CreatedAt { get; init; }

        // Parameterless constructor for AutoMapper and model binding
        public PlaceBetDTO() { }

        public PlaceBetDTO(Guid walletId, Guid gameId, decimal amount, int currencyId, DateTime createdAt)
        {
            WalletId = walletId;
            GameId = gameId;
            Amount = amount;
            CurrencyId = currencyId;
            CreatedAt = createdAt;
        }
    }
}
