namespace CatalogAPI.Events.Models
{
    public class EventEnvelope<T>
    {
        public string ?MessageType { get; set; }
        public T ?Message { get; set; }
    }
}
