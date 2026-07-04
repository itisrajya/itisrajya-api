using itisrajyaApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace itisrajyaApi.Services;

public class ChatSessionManager
{
	private readonly ConcurrentDictionary<string, Timer> _timers = new();

	private readonly ChatSessionStore _store;
	private readonly IHubContext<ChatHub> _hub;

	public ChatSessionManager(
		ChatSessionStore store,
		IHubContext<ChatHub> hub)
	{
		_store = store;
		_hub = hub;
	}

	/// <summary>
	/// Starts (or restarts) the inactivity timer.
	/// Every new message resets the timer.
	/// </summary>
	public void ResetTimer(string sessionId)
	{
		if (_timers.TryRemove(sessionId, out var oldTimer))
		{
			oldTimer.Dispose();
		}

		var timer = new Timer(async _ =>
		{

			await ExpireSession(sessionId);

		}, null,
		TimeSpan.FromSeconds(60),
		Timeout.InfiniteTimeSpan);

		_timers[sessionId] = timer;
	}

	/// <summary>
	/// Called when the session expires.
	/// </summary>
	public async Task ExpireSession(string sessionId)
	{
		await _hub.Clients
			.Group(sessionId)
			.SendAsync("SessionExpired");

		_store.DeleteSession(sessionId);

		if (_timers.TryRemove(sessionId, out var timer))
		{
			timer.Dispose();
		}

		Console.WriteLine($"Session expired: {sessionId}");
	}

	/// <summary>
	/// Called when chat is manually ended.
	/// </summary>
	public void RemoveTimer(string sessionId)
	{
		if (_timers.TryRemove(sessionId, out var timer))
		{
			timer.Dispose();
		}
	}
}