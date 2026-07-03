using System.Text.Json;

public class ChatSessionStore
{
	private readonly string _root;

	public ChatSessionStore(IHostEnvironment env)
	{
		_root = Path.Combine(env.ContentRootPath, "chat");
		Directory.CreateDirectory(_root);
	}

	public string CreateSession()
	{
		var id = Guid.NewGuid().ToString("N");
		var file = Path.Combine(_root, id + ".txt");
		File.WriteAllText(file, string.Empty);
		return id;
	}

	public void AppendMessage(string sessionId, string sender, string message)
	{
		if (string.IsNullOrWhiteSpace(sessionId))
			return;

		var file = Path.Combine(_root, sessionId + ".txt");
		if (!File.Exists(file))
		{
			File.WriteAllText(file, string.Empty);
		}

		var line = $"{DateTime.UtcNow:O} | {sender} | {message}{Environment.NewLine}";
		File.AppendAllText(file, line);
	}

	public List<string> ReadMessages(string sessionId)
	{
		if (string.IsNullOrWhiteSpace(sessionId))
			return new List<string>();

		var file = Path.Combine(_root, sessionId + ".txt");
		if (!File.Exists(file))
			return new List<string>();

		return File.ReadAllLines(file).ToList();
	}

	public void EndSession(string sessionId)
	{
		if (string.IsNullOrWhiteSpace(sessionId))
			return;

		var file = Path.Combine(_root, sessionId + ".txt");
		if (File.Exists(file))
			File.Delete(file);
	}
}
