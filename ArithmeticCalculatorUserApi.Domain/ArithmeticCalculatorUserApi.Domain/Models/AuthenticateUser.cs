namespace ArithmeticCalculatorUserApi.Domain.Models
{
    public class AuthenticateUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
}
