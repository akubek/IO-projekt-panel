using IO_Panel.Server.Configuration;
using IO_Panel.Server.Consumers;
using IO_Panel.Server.Data;
using IO_Panel.Server.Hubs;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using IO_Panel.Server.Repositories.Ef;
using IO_Panel.Server.Services.Automations;
using IO_Panel.Server.Services.Time;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Application entry point. Configures DI, auth, persistence (SQLite), messaging (RabbitMQ), and real-time updates (SignalR).

var builder = WebApplication.CreateBuilder(args);

// Load overrides from panel.ini (dev/prod friendly) and merge into IConfiguration.
var iniPath = Path.Combine(AppContext.BaseDirectory, "panel.ini");
var iniValues = IniFile.ReadKeyValues(iniPath);

if (iniValues.Count > 0)
{
    builder.Configuration.AddInMemoryCollection(iniValues);
}


// Allow the dev client origin (Vite) to call the API + SignalR hub with credentials.
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientDev", policy =>
    {
        var origin = builder.Configuration["Client:DevOrigin"] ?? "https://localhost:52795";
        policy
            .WithOrigins(origin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Bind URL (requires restart after change)
var urls = builder.Configuration["Panel:Urls"];
if (!string.IsNullOrWhiteSpace(urls) && string.IsNullOrWhiteSpace(builder.Configuration["ASPNETCORE_URLS"]))
{
    builder.WebHost.UseUrls(urls);
}

// JWT-based admin authentication (single admin user from configuration).
var jwtKey = builder.Configuration["AdminAuth:JwtKey"]
    ?? "dev-admin-auth-key-abcdefghijklmnoprstuwvxyz0123456789";

var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");

                var name = context.Principal?.Identity?.Name ?? "(no name)";
                logger.LogDebug("Token validated. Name={Name}", name);

                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                logger.LogDebug("Forbidden: {Path}", context.HttpContext.Request.Path);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// SQLite database used for configured devices, rooms, scenes, automations, time configuration and history.
var sqliteConnectionString = builder.Configuration.GetConnectionString("AppDb")
    ?? "Data Source=app.db";

Console.WriteLine($"SQLite connection: {sqliteConnectionString}");
Console.WriteLine($"Using ini: {iniPath}");
Console.WriteLine($"AdminAuth.Username: {builder.Configuration["AdminAuth:Username"]}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnectionString));

// register typed HttpClient for external simulated devices API
builder.Services.AddHttpClient<IDeviceApiClient, HttpDeviceApiClient>(client =>
{
    var baseUrl = builder.Configuration["DeviceApi:BaseUrl"] ?? "https://localhost:7075/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// RabbitMQ integration: listen for device updates and publish device commands to the simulator.
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DeviceUpdatedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        var vhost = builder.Configuration["RabbitMq:VirtualHost"] ?? "/";
        var user = builder.Configuration["RabbitMq:Username"] ?? "guest";
        var pass = builder.Configuration["RabbitMq:Password"] ?? "guest";
        var port = int.TryParse(builder.Configuration["RabbitMq:Port"], out var p) ? p : 5672;

        var normalizedVhost = vhost.TrimStart('/');
        var rabbitMqUri = new Uri($"rabbitmq://{host}:{port}/{normalizedVhost}");

        cfg.Host(rabbitMqUri, h =>
        {
            h.Username(user);
            h.Password(pass);
        });

        var receiveEndpoint = builder.Configuration["RabbitMq:ReceiveEndpoint"] ?? "device-updates";
        var bindExchange = builder.Configuration["RabbitMq:BindExchange"] ?? "device-updated";

        cfg.ReceiveEndpoint(receiveEndpoint, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Bind(bindExchange);
            e.ConfigureConsumer<DeviceUpdatedEventConsumer>(context);
        });
    });
});

builder.Services.AddScoped<IDeviceRepository, EfDeviceRepository>();
builder.Services.AddScoped<IRoomRepository, EfRoomRepository>();
builder.Services.AddScoped<ISceneRepository, EfSceneRepository>();
builder.Services.AddScoped<IAutomationRepository, EfAutomationRepository>();
builder.Services.AddScoped<IAutomationRunner, AutomationRunner>();
builder.Services.AddScoped<ITimeService, TimeService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();

builder.Services.AddSignalR();

// Background automation evaluator that periodically checks triggers and executes actions.
builder.Services.AddHostedService<IO_Panel.Server.Services.Automations.AutomationPeriodicEvaluator>();

var app = builder.Build();

// Apply pending EF Core migrations at startup (dev-friendly; avoid in strict production setups).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("ClientDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Real-time push of device updates to connected UI clients.
app.MapHub<DeviceUpdatesHub>("/hubs/device-updates")
    .RequireCors("ClientDev");

app.MapFallbackToFile("{*path:nonfile}", "/index.html");

app.Run();

