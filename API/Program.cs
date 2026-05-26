using Amazon.SQS;
using CatalogAPI.Extensions;
using CatalogAPI.Infrastructure.Data;
using CatalogAPI.Infrastructure.Repositories;
using CatalogAPI.Middlewares;
using MassTransit;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

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
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(host, "/", h =>
            {
                h.Username(username);
                h.Password(password);
            });

            cfg.ConfigureEndpoints(context);
        });
    }
});

// ======================
// AWS Config
// ======================
builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var AWS_USER = Environment.GetEnvironmentVariable("AWS_USER");
    var AWS_PASSWORD = Environment.GetEnvironmentVariable("AWS_PASSWORD");
    var AWS_SERVICE_URL = Environment.GetEnvironmentVariable("AWS_SERVICE_URL");

    if (AWS_USER == null || AWS_PASSWORD == null || AWS_SERVICE_URL == null)
    {
        throw new Exception("AWS SQS configuration is missing. Please set environment variables AWS_USER, AWS_PASSWORD and AWS_SERVICE_URL");
    }

    var config = new AmazonSQSConfig
    {
        ServiceURL = AWS_SERVICE_URL,
        UseHttp = true
    };

    return new AmazonSQSClient(AWS_USER, AWS_PASSWORD, config);
});

// ======================
// Mongo Config
// ======================
builder.Services.AddSingleton<AppDbContext>();

// ======================
// Redis Config
// ======================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
    options.InstanceName = "catalog-api";
});

builder.Services.AddScoped<IUserGameRepository, UserGameRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddHostedService<PaymentProcessedConsumerService>();

// ======================
var app = builder.Build();
// ======================

app.UseHttpMetrics();

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
app.MapMetrics();
app.Run();