using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace WebApplication2.Services
{
    public class QueuedEmailSender : BackgroundService
    {
        private readonly IBackgroundEmailQueue _queue;
        private readonly IEmailSender _sender;
        private readonly ILogger<QueuedEmailSender> _logger;

        public QueuedEmailSender(IBackgroundEmailQueue queue, IEmailSender sender, ILogger<QueuedEmailSender> logger)
        {
            _queue = queue;
            _sender = sender;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QueuedEmailSender started");

            while (!stoppingToken.IsCancellationRequested)
            {
                MimeMessage message = null!;
                try
                {
                    message = await _queue.DequeueAsync(stoppingToken);

                    if (message == null)
                        continue;

                    await _sender.SendAsync(message, stoppingToken);
                    _logger.LogInformation("Email sent to {To}", string.Join(',', message.To));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending queued email to {To}. Message will be lost.", 
                        message != null ? string.Join(',', message.To) : "unknown");
                    // Примечание: для продакшена рекомендуется добавить DLQ (Dead Letter Queue) 
                    // для сохранения неудачных сообщений в БД или файл для последующей обработки
                }
            }

            _logger.LogInformation("QueuedEmailSender stopped");
        }
    }
}
