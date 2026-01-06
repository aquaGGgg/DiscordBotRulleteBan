using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Middleware;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "BannedService API", Version = "v1" });
});

// CORS (AdminPanel dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", p =>
    {
        p.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// DbContext (PostgreSQL)
var cs = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

builder.Services.AddDbContext<BannedServiceDbContext>(opt =>
{
    opt.UseNpgsql(cs, npg =>
    {
        // Миграции в указанной сборке (у тебя Migrations в Infrastructure/Persistence/Migrations)
        npg.MigrationsAssembly(typeof(BannedServiceDbContext).Assembly.FullName);
    });

    // В проде лучше выключить, но в dev полезно
    opt.EnableDetailedErrors(builder.Environment.IsDevelopment());
    opt.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// Layers DI
builder.Services.AddInfrastructure();
builder.Services.AddApplication();

builder.Services.Configure<Application.BackgroundJobs.Options.WorkersOptions>(
    builder.Configuration.GetSection("Workers"));


// App
var app = builder.Build();

// Global error -> ProblemDetails
app.UseAppExceptionHandling();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "BannedService API v1");
    o.RoutePrefix = "swagger";
});

// Routing + CORS
app.UseRouting();
app.UseCors("DevCors");

// Controllers
app.MapControllers();

var autoMigrate = app.Configuration.GetValue<bool>("AutoMigrate");
if (autoMigrate)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BannedServiceDbContext>();
    db.Database.Migrate();
}

app.Run();
