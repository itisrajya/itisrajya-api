namespace itisrajyaApi.Models;

public class ChatSession
{
	public string SessionId { get; set; } = "";

	public DateTime CreatedAt { get; set; }

	public DateTime LastActivity { get; set; }

	public string Status { get; set; } = "active";

	public int NodeCount { get; set; }

	public List<ChatMessage> Messages { get; set; } = new();
}

public class ChatMessage
{
	public int Node { get; set; }

	public string SessionId { get; set; } = "";

	public string Sender { get; set; } = "";

	public string Message { get; set; } = "";
}