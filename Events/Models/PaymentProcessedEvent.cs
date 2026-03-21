namespace Events.Models;

public record PaymentProcessedEvent
{
    public required string Status { get; set; }
    public Guid GameId { get; set; }
    public decimal Price { get; set; }
    public required string UserId { get; set; }
}
