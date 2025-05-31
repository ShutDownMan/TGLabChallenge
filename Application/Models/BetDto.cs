namespace Application.Models
{
    public record BetDTO
    (
        Guid Id,
        Guid WalletId,
        Guid GameId,
        decimal Amount,
        decimal? Prize,
        int CurrencyId,
        DateTime CreatedAt
    );
}
