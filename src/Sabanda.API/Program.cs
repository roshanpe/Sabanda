using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Sabanda.API.Extensions;
using Sabanda.API.Middleware;
using Sabanda.API.Settings;
using Sabanda.Infrastructure.Extensions;
using Sabanda.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ─── Startup validation (fail fast) ───────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");

var qrKey = builder.Configuration["QrToken:SigningKey"];
if (string.IsNullOrEmpty(qrKey) || qrKey.Length < 32)
    throw new InvalidOperationException("QrToken:SigningKey must be at least 32 characters.");

// ─── Settings binding ──────────────────────────────────────────────────────────
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<QrTokenSettings>(builder.Configuration.GetSection("QrToken"));
builder.Services.Configure<DataRetentionSettings>(builder.Configuration.GetSection("DataRetention"));
builder.Services.Configure<RateLimiterSettings>(builder.Configuration.GetSection("RateLimiter"));

// ─── Infrastructure & Application ─────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ─── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

// ─── JWT Authentication + Authorization policies ───────────────────────────────
builder.Services.AddJwtAuthentication(builder.Configuration);

// ─── Rate limiting ─────────────────────────────────────────────────────────────
builder.Services.AddSabandaRateLimiting(builder.Configuration);

// ─── Hangfire (skip in Testing environment — Testcontainers provides its own DB) ──
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    builder.Services.AddHangfire(config =>
        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
              .UseSimpleAssemblyNameTypeSerializer()
              .UseRecommendedSerializerSettings()
              .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));
    builder.Services.AddHangfireServer();
}

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173", "http://localhost:5174")
     .AllowAnyHeader()
     .AllowAnyMethod()));

// ─── Swagger/OpenAPI ──────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Sabanda API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT bearer token"
    };
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ─── Database migration + seeding ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SabandaDbContext>();
    if ((app.Environment.IsDevelopment() ||
        Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true") &&
        !app.Environment.IsEnvironment("Testing"))
    {
        await db.Database.MigrateAsync();
        await DataSeeder.SeedAsync(db);
    }
}

// ─── Middleware pipeline (ORDER IS CRITICAL) ───────────────────────────────────
app.UseHttpsRedirection();

app.UseCors();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseMiddleware<TenantResolutionMiddleware>();  // must be before auth

app.UseRateLimiter();

app.UseAuthentication();

app.UseMiddleware<JtiValidationMiddleware>();     // must be after auth

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
    });
}

app.MapControllers();

app.Run();

// Partial class for integration test WebApplicationFactory access
public partial class Program { }
