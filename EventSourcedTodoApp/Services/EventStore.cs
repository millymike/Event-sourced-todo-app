using System.Text;
using EventSourcedTodoApp.Events;
using EventStore.Client;
using Newtonsoft.Json;

namespace EventSourcedTodoApp.Services
{
    public class EventStore : IEventStore
    {
        private readonly EventStoreClient _eventStoreClient;
        private readonly Dictionary<string, Type> _eventTypeMap;

        public EventStore(EventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
            _eventTypeMap = DiscoverAllEventTypesByInterface();
        }

        private Dictionary<string, Type> DiscoverAllEventTypesByInterface()
        {
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where typeof(IEvent).IsAssignableFrom((Type?)type) && !type.IsInterface
                select type).ToDictionary(type => type.Name);
        }

        public async Task WriteEvents(string streamId, IEnumerable<IEvent> events, CancellationToken ct)
        {
            await _eventStoreClient.AppendToStreamAsync(
                streamId,
                StreamState.Any,
                events.Select(x => new EventData(
                    Uuid.NewUuid(),
                    x.GetType().Name,
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(x))
                )), cancellationToken: ct);
        }

        public async Task SubscribeToAllStreams(Func<IEvent, Task> handler, CancellationToken ct)
        {
            await _eventStoreClient.SubscribeToAllAsync(FromAll.Start,
                async (subscription, resolvedEvent, cancellationToken) =>
                {
                    if (!_eventTypeMap.TryGetValue(resolvedEvent.Event.EventType, out Type? eventType))
                    {
                        return;
                    }

                    var eventData = JsonConvert.DeserializeObject(
                        Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span),
                        eventType
                    )!;
                    await handler((IEvent)eventData);
                }, cancellationToken: ct);
        }
    }
}