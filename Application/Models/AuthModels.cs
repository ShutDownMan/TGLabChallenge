namespace Application.Models
{
    public record LoginRequest(string Username, string Password);
    public record TokenRequest(string Token);
}
