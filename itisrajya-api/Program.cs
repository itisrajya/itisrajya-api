using itisrajyaApi.Endpoints;
using itisrajyaApi.Hubs;
using itisrajyaApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ChatSessionStore>();

builder.Services.AddSingleton<EmailService>();

builder.Services.AddSingleton<ChatSessionManager>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
	options.AddPolicy("angular", policy =>
	{
		policy
			.WithOrigins(
				"http://localhost:4200",
				"http://127.0.0.1:4200")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("angular");

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
	status = "ok"
}));

app.MapControllers();

app.MapChatEndpoints();

app.MapHub<ChatHub>("/chatHub");

app.MapFallbackToFile("index.html");

app.Run();