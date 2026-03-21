using CatalogAPI.Domain.Entities;
using CatalogAPI.Infrastructure.Repositories;
using Events.Models;
using MassTransit;

namespace CatalogAPI.Events.Consumers
{
    public class PaymentProcessedConsumer(IUserGameRepository userGameRepository) : IConsumer<PaymentProcessedEvent>
    {
        private readonly IUserGameRepository _userGameRepository = userGameRepository;

        public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        {
            var message = context.Message;
            var status = message.Status;

            if (status == "Approved") 
            {
                _userGameRepository.AddAsync(new UserGame(
                    Guid.NewGuid(),
                    Guid.Parse(message.UserId),
                    message.GameId,
                    message.Price
                ));

                Console.WriteLine("Pagamento aprovado e game adicionado ao catalogo");
            }
            
            return Task.CompletedTask;
        }
    }
}