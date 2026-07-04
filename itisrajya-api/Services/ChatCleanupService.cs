using itisrajyaApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace itisrajyaApi.Services;

public class ChatCleanupService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<ChatCleanupService> _logger;

	public ChatCleanupService(
		IServiceProvider serviceProvider,
		ILogger<ChatCleanupService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(
		CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();

				var store =
					scope.ServiceProvider.GetRequiredService<ChatSessionStore>();

				var hub =
					scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

				foreach (var sessionId in store.GetAllSessions())
				{
					var session = store.GetSession(sessionId);

					if (session == null)
						continue;

					if (!store.IsExpired(session))
						continue;

					_logger.LogInformation(
						"Expiring session {SessionId}",
						session.SessionId);

					await hub.Clients
						.Group(session.SessionId)
						.SendAsync(
							"SessionExpired",
							cancellationToken: stoppingToken);

					store.DeleteSession(session.SessionId);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Error while cleaning chat sessions.");
			}

			await Task.Delay(
				TimeSpan.FromSeconds(5),
				stoppingToken);
		}
	}
}