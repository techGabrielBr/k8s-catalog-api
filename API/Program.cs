using Amazon.SQS;
using CatalogAPI.Extensions;
using CatalogAPI.Infrastructure.Data;
using CatalogAPI.Infrastructure.Repositories;
using CatalogAPI.Infrastructure.Search;
using CatalogAPI.Middlewares;
using MassTransit;
using OpenSearch.Client;
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
    var AWS_SERVICE_URL = Environment.GetEnvironmentVariable("AWS_SERVICE_URL");

    if (AWS_SERVICE_URL != null)
    {
        var config = new AmazonSQSConfig
        {
            ServiceURL = AWS_SERVICE_URL,
            UseHttp = true
        };

        return new AmazonSQSClient(
            Environment.GetEnvironmentVariable("AWS_USER") ?? "teste",
            Environment.GetEnvironmentVariable("AWS_PASSWORD") ?? "teste",
            config
        );
    }

    return new AmazonSQSClient();
});

// ======================
// Mongo Config
// ======================
builder.Services.AddSingleton<AppDbContext>();

// ======================
// OpenSearch Config
// Amazon OpenSearch -> OPENSEARCH_URL (https) + OPENSEARCH_USERNAME/OPENSEARCH_PASSWORD
// ======================
builder.Services.AddSingleton<IOpenSearchClient>(_ =>
{
    var OPENSEARCH_URL = Environment.GetEnvironmentVariable("OPENSEARCH_URL");

    if (OPENSEARCH_URL == null)
    {
        throw new Exception("OpenSearch configuration is missing. Please set environment variable OPENSEARCH_URL");
    }

    var settings = new ConnectionSettings(new Uri(OPENSEARCH_URL))
        .DefaultIndex(GameSearchService.IndexName)
        .DisableDirectStreaming();

    var OPENSEARCH_USERNAME = Environment.GetEnvironmentVariable("OPENSEARCH_USERNAME");
    var OPENSEARCH_PASSWORD = Environment.GetEnvironmentVariable("OPENSEARCH_PASSWORD");

    if (!string.IsNullOrEmpty(OPENSEARCH_USERNAME))
    {
        settings = settings.BasicAuthentication(OPENSEARCH_USERNAME, OPENSEARCH_PASSWORD);
    }

    return new OpenSearchClient(settings);
});

builder.Services.AddSingleton<IGameSearchService, GameSearchService>();

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