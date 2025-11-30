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

// seed sample devices
var seedDevices =  new List<Device>
{
    new Device { Id = "dev-1", Name = "Sensor A", Type = "Sensor", Status = "Online", LastSeen = DateTime.UtcNow.AddMinutes(-1), Localization = "Living room" },
    new Device { Id = "dev-2", Name = "Lamp B",  Type = "Switch", Status = "Offline", LastSeen = DateTime.UtcNow.AddHours(-1), Localization = "Kitchen" },
    new Device { Id = "dev-3", Name = "Thermometer C",  Type = "Slider", Status = "Online", LastSeen = DateTime.UtcNow.AddMinutes(-1), Localization = "Garage" }
};

// register config store (in-memory for now)
builder.Services.AddSingleton<IDeviceConfigRepository, InMemoryDeviceConfigRepository>();

// Development: keep seeded in-memory repo to preserve state across requests
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IDeviceRepository>(sp => new InMemoryDeviceRepository(seedDevices));
}
else
{
    // Production / integration: use DeviceRepository which composes IDeviceApiClient + IDeviceConfigRepository
    builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
}

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

