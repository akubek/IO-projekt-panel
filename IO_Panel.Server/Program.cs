using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// register typed HttpClient for external simulated devices API
builder.Services.AddHttpClient<IDeviceApiClient, HttpDeviceApiClient>(client =>
{
    var baseUrl = builder.Configuration["DeviceApi:BaseUrl"] ?? "https://localhost:7075/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<IDeviceRepository, DeviceRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register PasswordHasher for AdminUser
builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();

var app = builder.Build();

// Seed the repository with sample device configs for development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var repo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
        var seedDevices = new List<Device>
        {
            new Device { Id = "dev-1", Name = "Sensor A", Type = "sensor", Status = "Online", LastSeen = DateTime.UtcNow.AddMinutes(-1), Localization = "Living room", Description = "Temperature sensor", State = new DeviceState { Value = 22.5, Unit = "°C" }, Config = new DeviceConfig { ReadOnly = true, Min = -40, Max = 125, Step = 0.1 }, IsConfigured = true },
            new Device { Id = "dev-2", Name = "Lamp B",  Type = "switch", Status = "Offline", LastSeen = DateTime.UtcNow.AddHours(-1), Localization = "Kitchen", Description = "Ceiling lamp", State = new DeviceState { Value = 0, Unit = null }, Config = new DeviceConfig { ReadOnly = false, Min = 0, Max = 1, Step = 1 }, IsConfigured = false },
            new Device { Id = "dev-3", Name = "Thermometer C",  Type = "slider", Status = "Online", LastSeen = DateTime.UtcNow.AddMinutes(-1), Localization = "Garage", Description = "Setpoint control", State = new DeviceState { Value = 50, Unit = "%" }, Config = new DeviceConfig { ReadOnly = false, Min = 0, Max = 100, Step = 1 }, IsConfigured = true }
        };

        foreach (var d in seedDevices)
        {
            try { repo.SaveConfigAsync(d.Id, d.Config).GetAwaiter().GetResult(); } catch { }
        }
    }
}


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

