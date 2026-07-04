namespace itisrajyaApi.Models;

public record ChatMessageRequest(
	string SessionId,
	string Sender,
	string Message);