namespace Application.Models
{
    public record LoginRequest(string UsernameOrEmail, string Password);
    public record TokenRequest(string Token);
}
