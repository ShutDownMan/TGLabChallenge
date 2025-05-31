namespace Application.Models
{
    public record LoginRequest(string Identifier, string Password);
    public record TokenRequest(string Token);
}
