using MimeKit;

namespace WebApplication2.Services
{
    public interface IEmailSender
    {
        Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default);
    }
}
