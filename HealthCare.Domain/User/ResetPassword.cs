namespace HealthCare.Domain.User
{
    public class ResetPassword
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}