using System.Threading.Channels;
using MimeKit;

namespace WebApplication2.Services
{
    public interface IBackgroundEmailQueue
    {
        ValueTask EnqueueAsync(MimeMessage message, CancellationToken cancellationToken = default);
        ValueTask<MimeMessage> DequeueAsync(CancellationToken cancellationToken = default);
    }

    public class BackgroundEmailQueue : IBackgroundEmailQueue
    {
        private readonly Channel<MimeMessage> _queue = Channel.CreateUnbounded<MimeMessage>();

        public async ValueTask EnqueueAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            await _queue.Writer.WriteAsync(message, cancellationToken);
        }

        public async ValueTask<MimeMessage> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
