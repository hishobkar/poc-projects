using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add CORS for React development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

app.UseCors("AllowReactApp");
app.UseWebSockets();

// Store connected clients
var clients = new List<WebSocket>();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        clients.Add(webSocket);
        
        Console.WriteLine($"Client connected. Total clients: {clients.Count}");
        
        try
        {
            await HandleWebSocketConnection(webSocket);
        }
        finally
        {
            clients.Remove(webSocket);
            Console.WriteLine($"Client disconnected. Total clients: {clients.Count}");
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

async Task HandleWebSocketConnection(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    var receiveResult = await webSocket.ReceiveAsync(
        new ArraySegment<byte>(buffer), CancellationToken.None);

    while (!receiveResult.CloseStatus.HasValue)
    {
        var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
        Console.WriteLine($"Received: {message}");

        // Broadcast message to all connected clients
        var broadcastMessage = new
        {
            type = "message",
            content = message,
            timestamp = DateTime.UtcNow.ToString("HH:mm:ss"),
            clientCount = clients.Count
        };

        var broadcastJson = JsonSerializer.Serialize(broadcastMessage);
        var broadcastBytes = Encoding.UTF8.GetBytes(broadcastJson);
        
        foreach (var client in clients.Where(c => c != webSocket && c.State == WebSocketState.Open))
        {
            try
            {
                await client.SendAsync(
                    new ArraySegment<byte>(broadcastBytes, 0, broadcastBytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch
            {
                // Client might be disconnected
            }
        }

        // Send confirmation back to sender
        var confirmation = new
        {
            type = "confirmation",
            content = "Message delivered",
            timestamp = DateTime.UtcNow.ToString("HH:mm:ss")
        };
        var confirmationJson = JsonSerializer.Serialize(confirmation);
        var confirmationBytes = Encoding.UTF8.GetBytes(confirmationJson);
        
        await webSocket.SendAsync(
            new ArraySegment<byte>(confirmationBytes, 0, confirmationBytes.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    await webSocket.CloseAsync(
        receiveResult.CloseStatus.Value,
        receiveResult.CloseStatusDescription,
        CancellationToken.None);
}

app.MapGet("/", () => "WebSocket Server Running");

app.Run();