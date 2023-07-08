namespace EventSourcedTodoApp.Events;

public class TodoStatusUpdatedEvent : IEvent
{
    public int Id { get; set; }
    public bool IsCompleted { get; set; }
}