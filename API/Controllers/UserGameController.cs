using CatalogAPI.API.Controllers.Models;
using CatalogAPI.Infrastructure.Repositories;
using Events.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatalogAPI.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/catalog")]
    public class UserGameController(IPublishEndpoint publishEndpoint, IUserGameRepository userGameRepository)
        : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
        private IUserGameRepository _userGameRepository = userGameRepository;

        [HttpPost("place-order")]
        public async Task<IActionResult> CatalogPlaceOrder(CatalogPlaceOrderRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return BadRequest("User not found");

            var userGuid = Guid.Parse(userId);

            var alreadyOwned = await _userGameRepository.ExistsAsync(userGuid, request.GameId);

            if (alreadyOwned)
            {
                return Conflict(new
                {
                    message = "Não foi possível concluir o pedido: Jogo já adquirido anteriormente"
                });
            }

            await _publishEndpoint.Publish(new OrderPlacedEvent
            {
                UserId = userId,
                GameId = request.GameId,
                Price = request.Price
            });

            return Ok("Pagamento em processamento");
        }
    }
}