using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Domain.Rounds;

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
<<<<<<< Updated upstream
        // �������� � ��������� ������ (� ���� Migrations � Infrastructure/Persistence/Migrations)
        npg.MigrationsAssembly(typeof(BannedServiceDbContext).Assembly.FullName);
    });

    // � ����� ����� ���������, �� � dev �������
=======
        npg.MigrationsAssembly(typeof(BannedServiceDbContext).Assembly.FullName);
    });

>>>>>>> Stashed changes
    opt.EnableDetailedErrors(builder.Environment.IsDevelopment());
    opt.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// Layers
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


// ===== INITIAL CONFIG SEED (RAW SQL, SAFE) =====
// ===== INITIAL CONFIG SEED (FINAL, WORKING) =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BannedServiceDbContext>();
    var cfg = app.Configuration.GetSection("InitialConfig");

    if (!cfg.Exists())
        throw new InvalidOperationException("InitialConfig section missing in appsettings.json");

    await db.Database.ExecuteSqlRawAsync(@"
        INSERT INTO config (
            id,
            ban_roulette_interval_seconds,
            ban_roulette_pick_count,
            ban_roulette_duration_min_seconds,
            ban_roulette_duration_max_seconds,
            ticket_roulette_interval_seconds,
            ticket_roulette_pick_count,
            ticket_roulette_tickets_min,
            ticket_roulette_tickets_max,
            eligible_role_id,
            jail_voice_channel_id,
            updated_at
        )
        SELECT
            1,
            {0}, {1}, {2}, {3},
            {4}, {5}, {6}, {7},
            {8}, {9},
            NOW()
        WHERE NOT EXISTS (
            SELECT 1 FROM config WHERE id = 1
        );
    ",
    cfg.GetValue<int>("BanRouletteIntervalSeconds"),
    cfg.GetValue<int>("BanRoulettePickCount"),
    cfg.GetValue<int>("BanRouletteDurationMinSeconds"),
    cfg.GetValue<int>("BanRouletteDurationMaxSeconds"),
    cfg.GetValue<int>("TicketRouletteIntervalSeconds"),
    cfg.GetValue<int>("TicketRoulettePickCount"),
    cfg.GetValue<int>("TicketRouletteTicketsMin"),
    cfg.GetValue<int>("TicketRouletteTicketsMax"),
    cfg.GetValue<string?>("EligibleRoleId"),
    cfg.GetValue<string?>("JailVoiceChannelId")
    );
}


// Health check
app.MapGet("/", () => "ServerIsLive");

app.Run();
