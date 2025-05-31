namespace Application.Models
{
    public record BetDto
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
