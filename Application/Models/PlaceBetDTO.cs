namespace Application.Models
{
    public record PlaceBetDTO
    (
        Guid Id,
        Guid WalletId,
        Guid GameId,
        decimal Amount,
        int CurrencyId,
        DateTime CreatedAt
    );
}
