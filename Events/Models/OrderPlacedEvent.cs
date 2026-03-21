namespace Events.Models;

public record OrderPlacedEvent
{
    public required string UserId { get; set; }
    public Guid GameId { get; set; }
    public decimal Price { get; set; }
}