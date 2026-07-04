namespace itisrajyaApi.Models;

public record ChatNotificationRequest(
	string AdminEmail,
	string ChatSessionId,
	string ChatLink);