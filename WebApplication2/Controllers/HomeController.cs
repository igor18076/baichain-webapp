using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Diagnostics;
using System.IO;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {

        private readonly IWebHostEnvironment _env;
        private readonly IBackgroundEmailQueue _emailQueue;
        private readonly SmtpOptions _smtpOptions;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IWebHostEnvironment env,
            IBackgroundEmailQueue emailQueue,
            IOptions<SmtpOptions> smtpOptions,
            ILogger<HomeController> logger)
        {
            _env = env;
            _emailQueue = emailQueue;
            _smtpOptions = smtpOptions.Value;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ContactFormViewModel model)
        {
            if (!model.Agree)
            {
                ModelState.AddModelError(nameof(model.Agree), "Необходимо согласие на обработку персональных данных.");
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var fromAddress = string.IsNullOrWhiteSpace(_smtpOptions.FromAddress) ? "order@baichein.ru" : _smtpOptions.FromAddress;
                var fromDisplay = string.IsNullOrWhiteSpace(_smtpOptions.FromDisplayName) ? "BaiChain команда" : _smtpOptions.FromDisplayName;

                var body = $@"Имя: {model.Name}
Телефон: {model.Phone}
Email: {model.Email}

Сообщение:
{model.Message}";

                var mime = new MimeMessage();
                mime.From.Add(new MailboxAddress(fromDisplay, fromAddress));
                mime.To.Add(MailboxAddress.Parse(fromAddress));
                mime.Subject = "Новая заявка на сайте BaiChain";
                mime.Body = new TextPart("plain") { Text = body };

                // Enqueue the message for background sending
                await _emailQueue.EnqueueAsync(mime);

                // Записываем информацию о заявке
                LogContact($"ENQUEUED: {model.Name}, {model.Phone}, {model.Email}");

                ViewBag.ContactSuccess = true;
                ViewBag.ShowKittenThanks = true;
                ModelState.Clear();
                return View(new ContactFormViewModel());
            }
            catch (Exception ex)
            {
                LogContact($"ERROR: {ex.Message} | {ex}");
                _logger.LogError(ex, "Ошибка отправки контактной формы");

                ViewBag.ContactError = "Не удалось отправить сообщение. Попробуйте ещё раз позже.";
                return View(model);
            }
        }

        private string GetLogPath()
        {
            return Path.Combine(_env.ContentRootPath ?? string.Empty, "App_Data", "contact.log");
        }

        private void EnsureLogDirectoryExists(string logPath)
        {
            var dir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private void LogContact(string message)
        {
            try
            {
                var logPath = GetLogPath();
                EnsureLogDirectoryExists(logPath);
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}";
                System.IO.File.AppendAllText(logPath, line);
                _logger.LogInformation("Contact form event: {Message}", message);
            }
            catch
            {
                // Не прерываем поток, если запись лога не удалась
            }
        }

        [HttpGet]
        public IActionResult DownloadContactLog()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var logPath = GetLogPath();
            if (!System.IO.File.Exists(logPath))
                return NotFound("Log file not found.");

            var bytes = System.IO.File.ReadAllBytes(logPath);
            return File(bytes, "text/plain", "contact.log");
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public IActionResult Services()
        {
            return View();
        }

        public IActionResult BlockchainPlatform()
        {
            return View();
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
