namespace CatalogAPI.API.Controllers.Models
{
    #region AUTH
    
    public record CatalogPlaceOrderRequest(
        Guid GameId,
        decimal Price
    );

    #endregion
}