namespace EventSourcedTodoApp.Events;

public class TodoStatusUpdatedEvent
{
    public Guid Id { get; set; }
    public bool IsCompleted { get; set; }
}