using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Polly;
using Polly.Retry;

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

            await _retryPolicy.ExecuteAsync(async ct =>
            {
                using var client = new SmtpClient();
                client.Timeout = 100_000;

                var secureOption = _opts.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

                await client.ConnectAsync(_opts.Host, _opts.Port, secureOption, ct);

                if (!string.IsNullOrEmpty(_opts.Username))
                {
                    await client.AuthenticateAsync(_opts.Username, _opts.Password, ct);
                }

                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);

            }, cancellationToken);
        }
    }
}
