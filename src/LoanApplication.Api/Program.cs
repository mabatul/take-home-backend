using LoanApplication.Core.Interfaces;
using LoanApplication.Core.Rules;
using LoanApplication.Infrastructure.Data;
using LoanApplication.Infrastructure.Events;
using LoanApplication.Infrastructure.Repositories;
using LoanApplication.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
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
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddSingleton<ApplicationEventPublisher>();
builder.Services.AddHttpClient<IExternalService, ExternalService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalService:BaseUrl"] ?? "http://localhost:3001");
});
builder.Services.AddHostedService<ApplicationEventProcessor>();
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