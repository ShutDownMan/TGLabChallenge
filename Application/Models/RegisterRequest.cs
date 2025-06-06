namespace Application.Models
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? InitialBalance { get; set; }
    }
}
