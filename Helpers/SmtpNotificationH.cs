using OneColumnEncoder.Models;
using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneColumnEncoder.Helpers;

public static class SmtpNotificationH
{
    public static async Task SendTestAsync(AppConfM.SmtpSettings settings, CancellationToken cancellationToken = default)
    {
        await SendAsync(
            settings,
            "1cenc SMTP test",
            "This is a test message from OneColumnEncoder.",
            cancellationToken);
    }

    public static async Task<bool> SendEncodingResultAsync(
        AppConfM.SmtpSettings settings,
        EncodingPipelineRequest request,
        bool success,
        TimeSpan duration,
        int? exitCode,
        string errorText,
        CancellationToken cancellationToken = default)
    {
        if (!ShouldNotify(settings, success, duration)) return false;

        string subject = success
            ? "1cenc encoding completed"
            : "1cenc encoding failed";
        string body = BuildEncodingBody(request, success, duration, exitCode, errorText);

        await SendAsync(settings, subject, body, cancellationToken);
        return true;
    }

    private static bool ShouldNotify(AppConfM.SmtpSettings settings, bool success, TimeSpan duration)
    {
        if (!IsConfigured(settings)) return false;

        if (success)
        {
            if (!settings.NotifyOnSuccess) return false;
            if (settings.NotifySuccessThresholdMin > 0
                && duration < TimeSpan.FromMinutes(settings.NotifySuccessThresholdMin))
                return false;
        }
        else
        {
            if (!settings.NotifyOnFailure) return false;
            if (settings.NotifyFailureThresholdMin > 0
                && duration < TimeSpan.FromMinutes(settings.NotifyFailureThresholdMin))
                return false;
        }

        if (!settings.NotifyOnNoInput || settings.NotifyAfterNoInputThresholdMin <= 0) return true;

        return UserIdleH.GetIdleTime() >= TimeSpan.FromMinutes(settings.NotifyAfterNoInputThresholdMin);
    }

    private static bool IsConfigured(AppConfM.SmtpSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.ServerUrl)
        && !string.IsNullOrWhiteSpace(settings.FromEmail)
        && !string.IsNullOrWhiteSpace(settings.ToEmail);

    private static async Task SendAsync(
        AppConfM.SmtpSettings settings,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        Validate(settings);

        using MailMessage message = new()
        {
            From = new MailAddress(settings.FromEmail.Trim()),
            Subject = subject,
            Body = body,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };
        message.To.Add(settings.ToEmail.Trim());

        using SmtpClient client = new(settings.ServerUrl.Trim(), settings.Port)
        {
            EnableSsl = settings.UseSSL,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(settings.Username))
            client.Credentials = new NetworkCredential(settings.Username, settings.Password);

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }

    private static void Validate(AppConfM.SmtpSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ServerUrl))
            throw new InvalidOperationException("SMTP server URL is required.");
        if (settings.Port <= 0 || settings.Port > 65535)
            throw new InvalidOperationException("SMTP port must be between 1 and 65535.");
        if (string.IsNullOrWhiteSpace(settings.FromEmail))
            throw new InvalidOperationException("SMTP from email address is required.");
        if (string.IsNullOrWhiteSpace(settings.ToEmail))
            throw new InvalidOperationException("SMTP to email address is required.");
    }

    private static string BuildEncodingBody(
        EncodingPipelineRequest request,
        bool success,
        TimeSpan duration,
        int? exitCode,
        string errorText)
    {
        StringBuilder body = new();
        body.AppendLine(success ? "Encoding completed." : "Encoding failed.");
        body.AppendLine(CultureInfo.InvariantCulture, $"Duration: {duration:hh\\:mm\\:ss}");
        body.AppendLine(CultureInfo.InvariantCulture, $"Exit code: {(exitCode?.ToString(CultureInfo.InvariantCulture) ?? "N/A")}");
        body.AppendLine($"Upstream: {request.UpstreamExeName}");
        body.AppendLine($"Encoder: {request.EncoderExeName}");
        body.AppendLine($"Source: {request.UpstreamInputPath}");
        body.AppendLine($"Output: {request.OutputPath}");

        if (!string.IsNullOrWhiteSpace(errorText))
        {
            body.AppendLine();
            body.AppendLine("Last process output:");
            body.AppendLine(TrimToTail(errorText, 4000));
        }

        return body.ToString();
    }

    private static string TrimToTail(string text, int maxLength) =>
        text.Length <= maxLength
            ? text
            : text[^maxLength..];
}
