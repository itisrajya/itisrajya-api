using System.Text.Json;
using itisrajyaApi.Models;

namespace itisrajyaApi.Services;

public class ChatSessionStore
{
	private readonly string _chatFolder;

	public ChatSessionStore(IHostEnvironment env)
	{
		_chatFolder = Path.Combine(env.ContentRootPath, "chat");
		Directory.CreateDirectory(_chatFolder);
	}

	private string GetPath(string sessionId)
	{
		return Path.Combine(_chatFolder, $"{sessionId}.json");
	}

	public ChatSession CreateSession()
	{
		var session = new ChatSession
		{
			SessionId = Guid.NewGuid().ToString("N"),
			CreatedAt = DateTime.UtcNow,
			LastActivity = DateTime.UtcNow,
			Status = "active",
			NodeCount = 0
		};

		SaveSession(session);

		return session;
	}

	public ChatSession? GetSession(string sessionId)
	{
		var path = GetPath(sessionId);

		if (!File.Exists(path))
			return null;

		var json = File.ReadAllText(path);

		return JsonSerializer.Deserialize<ChatSession>(json);
	}

	public void SaveSession(ChatSession session)
	{
		var path = GetPath(session.SessionId);

		File.WriteAllText(
			path,
			JsonSerializer.Serialize(
				session,
				new JsonSerializerOptions
				{
					WriteIndented = true
				}));
	}

	public ChatMessage AddMessage(
		string sessionId,
		string sender,
		string message)
	{
		var session = GetSession(sessionId)
					  ?? throw new Exception("Session not found.");

		var chatMessage = new ChatMessage
		{
			Node = session.Messages.Count + 1,
			SessionId = sessionId,
			Sender = sender,
			Message = message
		};

		session.Messages.Add(chatMessage);

		session.NodeCount = session.Messages.Count;

		session.LastActivity = DateTime.UtcNow;

		SaveSession(session);

		return chatMessage;
	}

	public bool DeleteSession(string sessionId)
	{
		var path = GetPath(sessionId);

		if (!File.Exists(path))
			return false;

		File.Delete(path);

		return true;
	}

	public bool IsExpired(ChatSession session)
	{
		return DateTime.UtcNow - session.LastActivity >
			   TimeSpan.FromSeconds(60);
	}

	public IEnumerable<string> GetAllSessions()
	{
		return Directory
			.EnumerateFiles(_chatFolder, "*.json")
			.Select(Path.GetFileNameWithoutExtension);
	}
}