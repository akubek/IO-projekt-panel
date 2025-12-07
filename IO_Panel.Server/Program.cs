using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// register typed HttpClient for external simulated devices API
builder.Services.AddHttpClient<IDeviceApiClient, HttpDeviceApiClient>(client =>
{
    var baseUrl = builder.Configuration["DeviceApi:BaseUrl"] ?? "https://localhost:7075/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        // Configuration for local RabbitMQ instance in Docker
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});


// Correctly register DeviceRepository with its dependencies
builder.Services.AddSingleton<IDeviceRepository>(sp =>
    new DeviceRepository(
        sp.GetRequiredService<IDeviceApiClient>(),
        sp.GetRequiredService<ILogger<DeviceRepository>>()
    ));

// Register the new repositories as singletons
builder.Services.AddSingleton<IRoomRepository>(sp => 
    new RoomRepository(sp.GetRequiredService<IDeviceRepository>()));
builder.Services.AddSingleton<ISceneRepository, SceneRepository>();
builder.Services.AddSingleton<IAutomationRepository, AutomationRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register PasswordHasher for AdminUser
builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");
app.Run();

