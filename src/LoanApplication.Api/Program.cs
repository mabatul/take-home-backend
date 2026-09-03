using LoanApplication.Core.Interfaces;
using LoanApplication.Core.Rules;
using LoanApplication.Infrastructure.Data;
using LoanApplication.Infrastructure.Events;
using LoanApplication.Infrastructure.Repositories;
using LoanApplication.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure SQLite
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Rule Engine
builder.Services.AddScoped<IApplicationRule>(sp =>
{
    var deniedStates = builder.Configuration.GetSection("DeniedStates").Get<string[]>() ?? new[] { "NY" };
    return new DeniedStatesRule(deniedStates);
});
builder.Services.AddScoped<IApplicationRule>(sp =>
{
    var blacklistedSsns = builder.Configuration.GetSection("BlacklistedSsns").Get<string[]>() ?? Array.Empty<string>();
    return new SsnBlacklistRule(blacklistedSsns);
});
builder.Services.AddScoped<RuleEngine>();

// Configure Repositories
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();

// Configure Event Publisher
builder.Services.AddSingleton<ApplicationEventPublisher>();

// Configure External Service
builder.Services.AddHttpClient<IExternalService, ExternalService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalService:BaseUrl"] ?? "http://localhost:3001");
});

// Configure Background Service
builder.Services.AddHostedService<ApplicationEventProcessor>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations on startup (unless disabled, e.g. in integration tests)
if (builder.Configuration.GetValue<bool?>("ApplyMigrationsOnStartup") ?? true)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.Run();

public partial class Program { }