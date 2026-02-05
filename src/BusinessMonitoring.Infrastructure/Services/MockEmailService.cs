using BusinessMonitoring.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace BusinessMonitoring.Infrastructure.Services;
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Email send retry {RetryCount} after {Delay}s",
                        retryCount, timeSpan.TotalSeconds
                    );
                }
            );
    }

    public async Task SendAsync(string to, string subject, string body, bool isHtml = true)
    {
        // Simulate email sending delay
        await Task.Delay(100);

        _logger.LogCritical(
            "📧 EMAIL SENT (Mock)\n" +
            "To: {To}\n" +
            "Subject: {Subject}\n" +
            "Body Length: {BodyLength} chars\n" +
            "Format: {Format}",
            to, subject, body.Length, isHtml ? "HTML" : "Plain Text"
        );
    }

    public async Task SendWithRetryAsync(string to, string subject, string body, bool isHtml = true)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            await SendAsync(to, subject, body, isHtml);
        });
    }
}
