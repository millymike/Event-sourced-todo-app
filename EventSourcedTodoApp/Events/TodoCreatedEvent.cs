namespace EventSourcedTodoApp.Events;

public class TodoCreatedEvent : IEvent
{
    public int Id { get; set; }
    public string Title { get; set; }
}