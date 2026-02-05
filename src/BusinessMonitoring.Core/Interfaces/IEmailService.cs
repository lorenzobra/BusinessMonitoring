namespace BusinessMonitoring.Core.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = true);

    Task SendWithRetryAsync(string to, string subject, string body, bool isHtml = true);
}
