using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Polly;
using Polly.Retry;
using System.Linq;

namespace WebApplication2.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opts;
        private readonly ILogger<MailKitEmailSender> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public MailKitEmailSender(IOptions<SmtpOptions> options, ILogger<MailKitEmailSender> logger)
        {
            _opts = options.Value;
            _logger = logger;

            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (ex, ts, retryCount, ctx) =>
                    {
                        _logger.LogWarning(ex, "Retry {RetryCount} sending email", retryCount);
                    });
        }

        public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_opts.Host))
                throw new InvalidOperationException("SMTP host is not configured.");

            _logger.LogInformation("Attempting to connect to SMTP server: {Host}:{Port}, UseStartTls: {UseStartTls}", 
                _opts.Host, _opts.Port, _opts.UseStartTls);

            await _retryPolicy.ExecuteAsync(async ct =>
            {
                using var client = new SmtpClient();
                client.Timeout = 100_000;

                // Определяем метод безопасного подключения
                var secureOption = _opts.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                
                // Если порт 465, используем SSL напрямую
                if (_opts.Port == 465)
                {
                    secureOption = SecureSocketOptions.SslOnConnect;
                }

                try
                {
                    _logger.LogInformation("Connecting to SMTP server {Host}:{Port} with {SecureOption}", 
                        _opts.Host, _opts.Port, secureOption);
                    
                    await client.ConnectAsync(_opts.Host, _opts.Port, secureOption, ct);
                    
                    _logger.LogInformation("Successfully connected to SMTP server");

                    if (!string.IsNullOrEmpty(_opts.Username))
                    {
                        _logger.LogInformation("Authenticating as {Username}", _opts.Username);
                        await client.AuthenticateAsync(_opts.Username, _opts.Password, ct);
                        _logger.LogInformation("Successfully authenticated");
                    }

                    await client.SendAsync(message, ct);
                    _logger.LogInformation("Email sent successfully to {Recipients}", 
                        string.Join(", ", message.To.Select(t => t.ToString())));
                    
                    await client.DisconnectAsync(true, ct);
                }
                catch (System.Net.Sockets.SocketException socketEx)
                {
                    _logger.LogError(socketEx, 
                        "Socket error connecting to {Host}:{Port}. Error code: {ErrorCode}, Message: {Message}. " +
                        "Check firewall settings, network connectivity, and SMTP server availability.", 
                        _opts.Host, _opts.Port, socketEx.SocketErrorCode, socketEx.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Host}:{Port}. Error: {Message}", 
                        _opts.Host, _opts.Port, ex.Message);
                    throw;
                }

            }, cancellationToken);
        }
    }
}
