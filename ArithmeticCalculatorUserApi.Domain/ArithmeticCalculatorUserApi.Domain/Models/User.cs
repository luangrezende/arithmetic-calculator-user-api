namespace ArithmeticCalculatorUserApi.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public string Password { get; set; }

        public IEnumerable<BankAccount> Accounts { get; set; }
    }
}
