using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RabbitMQConfirmMail.Controllers
{
    [Authorize]
    public class CarController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
