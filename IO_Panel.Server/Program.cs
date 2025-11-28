using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Przykładowe dane urządzeń, aktualnie stałe
var seedDevices =  new List<Device>
{
    new Device { Id = "dev-1", Name = "Sensor A", Type = "Sensor", Status = "Online", LastSeen = DateTime.UtcNow.AddMinutes(-1), Localization = "Living room" },
    new Device { Id = "dev-2", Name = "Lamp B",  Type = "Switch", Status = "Offline", LastSeen = DateTime.UtcNow.AddHours(-1), Localization = "Kitchen" },
    new Device { Id = "dev-3", Name = "Thermometer C",  Type = "Slider", Status = "Online", LastSeen = DateTime.UtcNow.AddMinutes(-1), Localization = "Garage" }
};


builder.Services.AddSingleton<IDeviceRepository>(sp => new InMemoryDeviceRepository(seedDevices));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
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
