namespace RabbitMQConfirmMail.Models
{
    public class CreateConfirmationCode
    {
        public string UserId { get; set; }
        public string ConfirmationCode { get; set; }
    }
}
