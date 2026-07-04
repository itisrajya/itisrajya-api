using itisrajyaApi.Hubs;
using itisrajyaApi.Services;
using Microsoft.AspNetCore.SignalR;
using itisrajyaApi.Models;

namespace itisrajyaApi.Endpoints;

public static class ChatEndpoints
{
	public static void MapChatEndpoints(this WebApplication app)
	{
		app.MapPost("/chat/start", StartSession);

		app.MapPost("/chat/message", SendMessage);

		app.MapGet("/chat/messages/{sessionId}", GetMessages);

		app.MapPost("/chat/end", EndSession);

		app.MapPost("/chat/send-chat-notification", SendNotification);
	}

	private static IResult StartSession(ChatSessionStore store, ChatSessionManager sessionManager)
	{
		var session = store.CreateSession();

		sessionManager.ResetTimer(session.SessionId);

		return Results.Ok(new
		{
			success = true,
			sessionId = session.SessionId
		});
	}

	private static async Task<IResult> SendMessage(
		ChatMessageRequest request,
		ChatSessionStore store,
		ChatSessionManager sessionManager,
		IHubContext<ChatHub> hub)
	{
		var message = store.AddMessage(
			request.SessionId,
			request.Sender,
			request.Message);

		sessionManager.ResetTimer(request.SessionId);

		await hub.Clients
			.Group(request.SessionId)
			.SendAsync("ReceiveMessage", message);

		return Results.Ok(new
		{
			success = true
		});
	}

	private static IResult GetMessages(
		string sessionId,
		ChatSessionStore store)
	{
		var session = store.GetSession(sessionId);

		if (session == null)
		{
			return Results.NotFound(new
			{
				success = false,
				expired = true
			});
		}

		return Results.Ok(session);
	}

	private static IResult EndSession(
		EndSessionRequest request,
		ChatSessionStore store,
		ChatSessionManager sessionManager)
	{
		store.DeleteSession(request.SessionId);

		sessionManager.RemoveTimer(request.SessionId);

		return Results.Ok(new
		{
			success = true
		});
	}

	private static async Task<IResult> SendNotification(
		ChatNotificationRequest request,
		EmailService emailService)
	{
		await emailService.SendNotificationAsync(request);

		return Results.Ok(new
		{
			success = true
		});
	}
}