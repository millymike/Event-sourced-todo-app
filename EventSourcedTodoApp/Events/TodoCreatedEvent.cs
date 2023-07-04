namespace EventSourcedTodoApp.Events;

public class TodoCreatedEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; }
}