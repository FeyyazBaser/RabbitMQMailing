namespace RabbitMQConfirmMail.Models
{
    public class ConfirmEmail
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string ConfirmationCode { get; set; }
    }
}
