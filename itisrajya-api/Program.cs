using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables();

builder.Services.AddSingleton<ChatSessionStore>();
builder.Services.AddCors(options =>
{
	options.AddPolicy("angular", policy =>
	{
		policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("angular");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/chat/send-chat-notification", async (ChatNotificationRequest request) =>
{
	Console.WriteLine($"[EmailRequest] adminEmail={request.AdminEmail} sessionId={request.ChatSessionId} chatLink={request.ChatLink}");

	if (request == null
		|| string.IsNullOrWhiteSpace(request.AdminEmail)
		|| string.IsNullOrWhiteSpace(request.ChatSessionId)
		|| string.IsNullOrWhiteSpace(request.ChatLink))
	{
		return Results.BadRequest(new { success = false, message = "Request payload is invalid." });
	}

	try
	{
		await SendNotificationEmailAsync(app.Configuration, request);
		Console.WriteLine("[EmailRequest] email sent successfully");
		return Results.Ok(new { success = true, message = "Email sent." });
	}
	catch (Exception ex)
	{
		Console.WriteLine($"[EmailRequest] email send failed: {ex}");
		return Results.Problem(detail: ex.Message, statusCode: 500);
	}
});
 
app.MapPost("/chat/start", (HttpContext http) =>
{
	var store = http.RequestServices.GetRequiredService<ChatSessionStore>();
	var sessionId = store.CreateSession();

	// Create chat folder if it doesn't exist
	var chatFolder = Path.Combine(Directory.GetCurrentDirectory(), "chat");
	Directory.CreateDirectory(chatFolder);

	// File path: chat/{sessionId}.json
	var filePath = Path.Combine(chatFolder, $"{sessionId}.json");

	// Initial JSON content
	var sessionData = new
	{
		SessionId = sessionId,
		CreatedAt = DateTime.UtcNow,
		NodeCount = 0,
		Messages = new List<object>()
	};

	var json = JsonSerializer.Serialize(sessionData, new JsonSerializerOptions
	{
		WriteIndented = true
	});

	File.WriteAllText(filePath, json);

	return Results.Ok(new
	{
		success = true,
		sessionId
	});
});

app.MapPost("/chat/message", (ChatMessageRequest request, HttpContext http) =>
{
	var store = http.RequestServices.GetRequiredService<ChatSessionStore>();
	store.AppendMessage(request.SessionId, request.Sender, request.Message);

	// Path to session file
	var chatFolder = Path.Combine(Directory.GetCurrentDirectory(), "chat");
	var filePath = Path.Combine(chatFolder, $"{request.SessionId}.json");

	if (!File.Exists(filePath))
	{
		return Results.NotFound(new
		{
			success = false,
			message = "Session file not found."
		});
	}

	// Read JSON
	var json = File.ReadAllText(filePath);
	var root = JsonNode.Parse(json)!.AsObject();

	// Ensure Messages array exists
	if (root["Messages"] == null)
	{
		root["Messages"] = new JsonArray();
	}

	var messages = root["Messages"]!.AsArray();

	// Next node number
	int nextNode = messages.Count + 1;

	// Append new message
	messages.Add(new JsonObject
	{
		["node"] = nextNode,
		["sessionId"] = request.SessionId,
		["sender"] = request.Sender,
		["message"] = request.Message
	});

	root["NodeCount"] = messages.Count;

	// Save file
	File.WriteAllText(
		filePath,
		root.ToJsonString(new JsonSerializerOptions
		{
			WriteIndented = true
		}));

	return Results.Ok(new { success = true });
});

app.MapGet("/chat/messages/{sessionId}", (string sessionId) =>
{
	var chatFolder = Path.Combine(Directory.GetCurrentDirectory(), "chat");
	var filePath = Path.Combine(chatFolder, $"{sessionId}.json");

	if (!File.Exists(filePath))
	{
		return Results.NotFound(new
		{
			success = false,
			message = $"Session '{sessionId}' not found."
		});
	}

	// Read and deserialize the JSON file
	var json = File.ReadAllText(filePath);
	var sessionData = JsonSerializer.Deserialize<object>(json);

	return Results.Ok(sessionData);
});

app.MapPost("/chat/end", (EndSessionRequest request, HttpContext http) =>
{
	var store = http.RequestServices.GetRequiredService<ChatSessionStore>();
	store.EndSession(request.SessionId);

	// Path to the session file
	var chatFolder = Path.Combine(Directory.GetCurrentDirectory(), "chat");
	var filePath = Path.Combine(chatFolder, $"{request.SessionId}.json");

	// Delete the file if it exists
	if (File.Exists(filePath))
	{
		File.Delete(filePath);
	}

	return Results.Ok(new
	{
		success = true
	});
});

app.MapFallbackToFile("index.html");

app.Run();

static async Task SendNotificationEmailAsync(IConfiguration config, ChatNotificationRequest request)
{
	var gmailUser = config["EmailSettings:GmailUser"];
	var gmailAppPassword = config["EmailSettings:GmailAppPassword"];

	if (string.IsNullOrWhiteSpace(gmailUser) || string.IsNullOrWhiteSpace(gmailAppPassword))
	{
		throw new InvalidOperationException("GMAIL_USER and GMAIL_APP_PASSWORD must be configured.");
	}

	Console.WriteLine($"[EmailSend] using {gmailUser}");

	using var message = new MailMessage
	{
		From = new MailAddress(gmailUser, "Terminal Chat Notification"),
		Subject = $"New chat session started: {request.ChatSessionId}",
		Body = $@"Hello Admin,

A new terminal chat session was started.

Session ID: {request.ChatSessionId}
Chat link: {request.ChatLink}

Open the link to join the session.
",
		IsBodyHtml = false
	};

	message.To.Add(request.AdminEmail);

	using var smtpClient = new SmtpClient("smtp.gmail.com", 587)
	{
		UseDefaultCredentials = false,
		Credentials = new NetworkCredential(gmailUser, gmailAppPassword),
		EnableSsl = true,
		Timeout = 10000
	};

	await smtpClient.SendMailAsync(message);
}

record ChatNotificationRequest(string AdminEmail, string ChatSessionId, string ChatLink);
record ChatMessageRequest(string SessionId, string Sender, string Message);
record EndSessionRequest(string SessionId);
