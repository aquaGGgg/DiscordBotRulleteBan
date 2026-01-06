using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Controllers
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(o =>
    {
        // Без "магического" автоматического 400 — дальше будем валидировать явно (шаги 5-6)
        o.SuppressModelStateInvalidFilter = true;
    });

// ProblemDetails (единый формат ошибок)
builder.Services.AddProblemDetails();

// Swagger (удобно в dev)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
var connStr = builder.Configuration.GetConnectionString("Default")
             ?? throw new InvalidOperationException("Missing connection string: ConnectionStrings:Default");

builder.Services.AddDbContext<BannedServiceDbContext>(opt =>
{
    opt.UseNpgsql(connStr, npgsql =>
    {
        // Миграции будут в сборке Infrastructure (в нашем случае — в этой же сборке BackendDSBot)
        npgsql.MigrationsAssembly(typeof(BannedServiceDbContext).Assembly.FullName);
    });

    opt.EnableDetailedErrors();
});

// HealthChecks (минимально)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BannedServiceDbContext>();

var app = builder.Build();

// Global exception handler => ProblemDetails
app.UseExceptionHandler();

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();


app.Run();

// для тестов / будущих интеграционных тестов
public partial class Program { }
