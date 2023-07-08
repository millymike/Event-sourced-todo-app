using EventSourcedTodoApp.Events;

namespace EventSourcedTodoApp.Services;

public interface IEventStore
{
    Task WriteEvents(string streamId, IEnumerable<IEvent> events, CancellationToken ct);
    Task SubscribeToAllStreams(Func<IEvent, Task> handler, CancellationToken ct);
}