using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQConfirmMail.Models;
using RabbitMQConfirmMail.Services;
using System.IO;
using System.Text;
using System.Text.Json;



namespace RabbitMQConfirmMail.Controllers
{
    public class AccountController : Controller
    {

        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;
        private AppDbContext _appDbContext;
        private RabbitMQPublisher _rabbitMQPublisher;
        private RabbitMQClientService _rabbitMQClientService;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AppDbContext appDbContext,
            RabbitMQPublisher rabbitMQPublisher, RabbitMQClientService rabbitMQClientService)
        {

            _rabbitMQClientService = rabbitMQClientService;
            _rabbitMQPublisher = rabbitMQPublisher;
            _appDbContext = appDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = registerViewModel.UserName,
                    Email = registerViewModel.Email
                };



                IdentityResult result = _userManager.CreateAsync(user, registerViewModel.Password).Result;
                Random generator = new Random();
                var confirmationCode = generator.Next(0, 1000000).ToString("D6");

                _appDbContext.ConfirmEmail.Add(new ConfirmEmail
                {
                    UserId = user.Id,
                    ConfirmationCode = confirmationCode

                });
                _appDbContext.SaveChanges();

                _rabbitMQPublisher.Publish(new CreateConfirmationCode
                {
                    UserId = user.Id,
                    ConfirmationCode = confirmationCode

                });

                if (result.Succeeded)
                {
                    var channel = _rabbitMQClientService.Connect();
                    channel.BasicQos(0, 1, false);
                    var consumer = new AsyncEventingBasicConsumer(channel);
                    channel.BasicConsume(RabbitMQClientService.queueName, true, consumer);
                    consumer.Received += Consumer_Received;
                    return RedirectToAction("ConfirmMail", "Account");
                }

            }

            return View(registerViewModel);
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult ConfirmMail()
        {

            return View();
        }

        public IActionResult VerifyMailCode(string confirmationCode)
        {
            var confirm = _appDbContext.ConfirmEmail.First(x => x.ConfirmationCode == confirmationCode);
            if (confirm != null)
            {
                var confirmEmail = _appDbContext.Users.Where(x => x.Id == confirm.UserId).First();
                confirmEmail.EmailConfirmed = true;
                _appDbContext.SaveChanges();
            }
            return RedirectToAction("Login");
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var createConfirmationCode = JsonSerializer.Deserialize<CreateConfirmationCode>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            System.IO.File.WriteAllText("Confirmation.txt", String.Empty);
            System.IO.File.AppendAllText("Confirmation.txt", createConfirmationCode.ConfirmationCode);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var result = _signInManager.PasswordSignInAsync(loginViewModel.UserName,
                    loginViewModel.Password, loginViewModel.RememberMe, false).Result;
                var userByName = await _appDbContext.Users.FirstAsync(x => x.UserName == loginViewModel.UserName);

                if (result.Succeeded && userByName.EmailConfirmed)
                {
                    return RedirectToAction("Index", "Car");
                }

                ModelState.AddModelError("", "Invalid login!");
            }

            return View(loginViewModel);
        }


        public ActionResult LogOff()
        {
            _signInManager.SignOutAsync().Wait();
            return RedirectToAction("Login");
        }
    }

}

