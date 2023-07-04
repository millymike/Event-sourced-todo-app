using System.Text;
using System.Text.Json;
using EventStore.Client;
using Grpc.Net.Client;

namespace EventSourcedTodoApp.Services;

public class EventStoreService : IDisposable
{
    private readonly EventStoreClient _eventStoreClient;
    private readonly string _streamName = "todo-events";

    public EventStoreService(string connectionString)
    {
        _eventStoreClient = new EventStoreClient(
            new EventStoreClientSettings
            {
                ConnectivitySettings =
                {
                    Address = new Uri("http://localhost:2113")
                },
            });

    }

    public async Task SaveEventAsync(object @event)
    {
        var eventData = new EventData(
            Uuid.NewUuid(),
            @event.GetType().Name,
            JsonSerializer.SerializeToUtf8Bytes(@event),
            Encoding.UTF8.GetBytes("{}")
        );

        await _eventStoreClient.AppendToStreamAsync(_streamName, StreamState.Any, new[] { eventData });
    }

    public void Dispose()
    {
        _eventStoreClient?.Dispose();
    }
}
