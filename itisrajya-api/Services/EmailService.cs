using System.Net;
using System.Net.Mail;
using itisrajyaApi.Models;

namespace itisrajyaApi.Services;

public class EmailService
{
	private readonly IConfiguration _config;

	public EmailService(IConfiguration config)
	{
		_config = config;
	}

	public async Task SendNotificationAsync(ChatNotificationRequest request)
	{
		var gmailUser = _config["EmailSettings:GmailUser"];
		var gmailPassword = _config["EmailSettings:GmailAppPassword"];

		if (string.IsNullOrWhiteSpace(gmailUser) ||
			string.IsNullOrWhiteSpace(gmailPassword))
		{
			throw new InvalidOperationException(
				"Email settings are missing.");
		}

		using var message = new MailMessage
		{
			From = new MailAddress(
				gmailUser,
				"Terminal Chat Notification"),

			Subject = $"New chat session started: {request.ChatSessionId}",

			Body = $"""
Hello Admin,

A new terminal chat session was started.

Session ID:
{request.ChatSessionId}

Open chat:
{request.ChatLink}

""",

			IsBodyHtml = false
		};

		message.To.Add(request.AdminEmail);

		using var smtp = new SmtpClient("smtp.gmail.com", 587)
		{
			Credentials = new NetworkCredential(
				gmailUser,
				gmailPassword),

			EnableSsl = true,
			UseDefaultCredentials = false
		};

		await smtp.SendMailAsync(message);
	}
}