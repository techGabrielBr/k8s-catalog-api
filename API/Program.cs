using CatalogAPI.Events.Consumers;
using CatalogAPI.Extensions;
using CatalogAPI.Infrastructure.Data;
using CatalogAPI.Infrastructure.Repositories;
using CatalogAPI.Middlewares;
using Events.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ======================
// Database (Sqlite)
// ======================
var defaultConnection = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(defaultConnection)
);

// ======================
// JWT
// ======================
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// ======================
// Controllers + Swagger
// ======================
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Catalog API",
        Version = "v1"
    });
});

builder.Services.AddScoped<IUserGameRepository, UserGameRepository>();

// ======================
// MassTransit Config
// ======================
builder.Services.AddMassTransit(x =>
{
    var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
    var username = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
    var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
    var paymentQueue = Environment.GetEnvironmentVariable("PAYMENT_QUEUE");

    if (host == null || username == null || password == null || paymentQueue == null)
    {
        throw new Exception("RabbitMQ configuration is missing. Please set environment variables");
    }
    else
    {
        x.AddConsumer<PaymentProcessedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(host, "/", h =>
            {
                h.Username(username);
                h.Password(password);
            });

            cfg.ReceiveEndpoint(paymentQueue, e =>
            {
                e.ConfigureConsumer<PaymentProcessedConsumer>(context);
            });
        });
    }
});

// ======================
var app = builder.Build();
// ======================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();