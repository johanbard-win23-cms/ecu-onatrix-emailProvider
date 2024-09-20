using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using ecu_onatrix_emailProvider.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ecu_onatrix_emailProvider.Functions;

public class EmailSender(ILogger<EmailSender> logger, EmailClient emailClient)
{
    private readonly ILogger<EmailSender> _logger = logger;
    private readonly EmailClient _emailClient = emailClient;

    [Function(nameof(EmailSender))]
    public async Task Run(
        [ServiceBusTrigger("email_request", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            var emailRequest = UnpackEmailRequest(message);
            if (emailRequest != null && !string.IsNullOrEmpty(emailRequest.To))
            {
                if (SendEmail(emailRequest))
                    await messageActions.CompleteMessageAsync(message);
                else
                    _logger.LogError($"ERROR : EmailSender.Run :: Email not sent, SendEmail returned false");
            }
            else
            {
                _logger.LogError($"ERROR : EmailSender.Run :: Email not sent, emailRequest not satisfactory");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.Run :: {ex.Message}");
        }
    }

    public EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(message.Body.ToString());
            if (emailRequest != null && emailRequest.To != null && emailRequest.Subject != null && emailRequest.HtmlContent != null && emailRequest.PlainTextContent != null) 
                return emailRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.UnpackEmailRequest() :: {ex.Message}");
        }

        return null!;
    }

    public bool SendEmail(EmailRequest emailRequest)
    {
        try
        {
            var result = _emailClient.Send(
                WaitUntil.Completed,
                senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                recipientAddress: emailRequest.To,
                subject: emailRequest.Subject,
                htmlContent: emailRequest.HtmlContent,
                plainTextContent: emailRequest.PlainTextContent);

            if (result.HasCompleted)
                return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.SendEmailAsync() :: {ex.Message}");
        }

        return false;
    }
}