using Arcraven.Avalonia.ResourcesLib.Models;

namespace Arcraven.Avalonia.ResourcesLib.Providers;

public interface IEventProvider : IDisposable
{
    /// <summary>
    /// Fired when a new event is ingested from the source.
    /// </summary>
    event EventHandler<Event> EventReceived;

    /// <summary>
    /// Starts the ingestion process (connects to DB, subscribes to Topic, etc.).
    /// </summary>
    Task ConnectAsync(CancellationToken ct);

    /// <summary>
    /// Optional: Retreive historical data before the stream starts.
    /// </summary>
    Task<IEnumerable<Event>> GetHistoryAsync(DateTimeOffset from, DateTimeOffset to);
}