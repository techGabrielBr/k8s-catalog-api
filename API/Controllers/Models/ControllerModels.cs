namespace CatalogAPI.API.Controllers.Models
{
    #region AUTH
    
    public record CatalogPlaceOrderRequest(
        Guid GameId,
        decimal Price
    );

    public record CreateGameRequest(
        string Name,
        string Description,
        decimal Price,
        string CoverUrl
    );

    public record UpdateGameRequest(
        string Name,
        string Description,
        decimal Price,
        string CoverUrl
    );

    #endregion
}