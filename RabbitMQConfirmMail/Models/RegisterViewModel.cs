using System.ComponentModel.DataAnnotations;

namespace RabbitMQConfirmMail.Models
{
    public class RegisterViewModel
    {

        [Required]
        public string UserName { get; set; }
             
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Girilen şifreler eşleşmemektedir!..")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Email { get; set; }


    }
}
