using System.Text;
using IO_Panel.Server.Consumers;
using IO_Panel.Server.Hubs;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientDev", policy =>
    {
        policy
            .WithOrigins("https://localhost:52795")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Dev-only signing key (must be 32+ bytes for HS256 safety)
const string jwtKey = "dev-admin-auth-key-abcdefghijklmnoprstuwvxyz0123456789";
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
                var user = context.Principal;

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");

                var name = user?.Identity?.Name ?? "(no name)";
                logger.LogInformation("Token validated. Name={Name}", name);

                if (user is not null)
                {
                    foreach (var claim in user.Claims)
                    {
                        logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }
                }

                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                logger.LogWarning("Forbidden: {Path}", context.HttpContext.Request.Path);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

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
    x.AddConsumer<DeviceUpdatedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("device-updates", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Bind("device-updated");
            e.ConfigureConsumer<DeviceUpdatedEventConsumer>(context);
        });
    });
});

builder.Services.AddSingleton<IDeviceRepository>(sp =>
    new DeviceRepository(
        sp.GetRequiredService<IDeviceApiClient>(),
        sp.GetRequiredService<ILogger<DeviceRepository>>()
    ));

builder.Services.AddSingleton<IRoomRepository>(sp =>
    new RoomRepository(sp.GetRequiredService<IDeviceRepository>()));
builder.Services.AddSingleton<ISceneRepository, SceneRepository>();
builder.Services.AddSingleton<IAutomationRepository, AutomationRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();

builder.Services.AddSignalR();

var app = builder.Build();

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

app.MapHub<DeviceUpdatesHub>("/hubs/device-updates")
    .RequireCors("ClientDev");

app.MapFallbackToFile("{*path:nonfile}", "/index.html");

app.Run();

