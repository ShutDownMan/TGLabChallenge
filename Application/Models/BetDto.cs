namespace Application.Models
{
    public record BetDTO
    (
        Guid Id,
        Guid PlayerId,
        decimal Amount,
        int StatusId,
        decimal? Prize,
        int CurrencyId,
        DateTime CreatedAt
    );
}
