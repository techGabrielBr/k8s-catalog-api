using Amazon.SQS;
using Amazon.SQS.Model;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Events.Models;
using CatalogAPI.Infrastructure.Repositories;
using Events.Models;
using System.Text.Json;

public class PaymentProcessedConsumerService : BackgroundService
{
    private readonly IAmazonSQS _sqs;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _queueUrl;

    public PaymentProcessedConsumerService(
        IAmazonSQS sqs,
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        var QUEUE_URL = Environment.GetEnvironmentVariable("QUEUE_SQS_URL");

        if (QUEUE_URL == null)
        {
            throw new ArgumentNullException("QUEUE_URL", "A URL da fila SQS não está configurada. Por favor, defina a variável de ambiente QUEUE_URL.");
        }

        _sqs = sqs;
        _scopeFactory = scopeFactory;
        _queueUrl = QUEUE_URL;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 10
                }, stoppingToken);

                if (response?.Messages != null && response.Messages.Count > 0)
                {
                    foreach (var message in response.Messages)
                    {
                        try
                        {
                            await HandleMessage(message.Body);

                            await _sqs.DeleteMessageAsync(new DeleteMessageRequest
                            {
                                QueueUrl = _queueUrl,
                                ReceiptHandle = message.ReceiptHandle
                            }, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na comunicação com o SQS: {ex.Message}");
            }

            await Task.Delay(500, stoppingToken);
        }
    }

    private async Task HandleMessage(string body)
    {
        try
        {
            var envelope =
                JsonSerializer.Deserialize<EventEnvelope<PaymentProcessedEvent>>(body);

            var evt = envelope?.Message;

            if (evt == null)
            {
                Console.WriteLine("Evento inválido");
                return;
            }

            if (evt.Status == "Approved")
            {
                using var scope = _scopeFactory.CreateScope();

                var repo = scope.ServiceProvider.GetRequiredService<IUserGameRepository>();

                await repo.AddAsync(new UserGame(
                    Guid.NewGuid(),
                    Guid.Parse(evt.UserId),
                    evt.GameId,
                    evt.Price
                ));

                Console.WriteLine("Game adicionado ao catálogo");
            }
            else
            {
                Console.WriteLine($"Pagamento rejeitado: {evt.Status}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parse error: {ex.Message}");
        }
    }
}