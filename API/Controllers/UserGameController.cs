using CatalogAPI.API.Controllers.Models;
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
    public class UserGameController (IPublishEndpoint publishEndpoint) : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        [HttpPost("place-order")]
        public async Task<IActionResult> CatalogPlaceOrder(CatalogPlaceOrderRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return BadRequest("User not found");
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
