using AmusementPark;
using AmusementPark.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Строка подключения к PostgreSQL (такая же, как в консольном приложении)
var connectionString = "Host=localhost;Port=5433;Database=AmusementParkDB;Username=postgres;Password=admin";

builder.Services.AddDbContextFactory<ApplicationContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<WebSocketHandler>();

var app = builder.Build();

// Создание/пересоздание БД при запуске
using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationContext>>();
    using var db = dbContextFactory.CreateDbContext();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    Console.WriteLine("База данных готова.");
}

app.UseWebSockets();

// WebSocket-эндпоинт
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var handler = context.RequestServices.GetRequiredService<WebSocketHandler>();
        await handler.HandleAsync(context, webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Простой HTTP-эндпоинт для проверки
app.MapGet("/", () => "Amusement Park WebSocket Server is running. Connect to /ws");

Console.WriteLine("Сервер запущен на http://localhost:5000");
app.Run("http://localhost:5000");