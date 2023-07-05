using EventSourcedTodoApp.Events;
using EventSourcedTodoApp.Models;
using EventSourcedTodoApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcedTodoApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly EventStoreService _eventStoreService;

    public TodoController(EventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTodo( TodoItem todoItem)
    {
        // Save the item creation event to EventStoreDB
        var todoCreatedEvent = new TodoCreatedEvent { Id = todoItem.Id, Title = todoItem.Title };
        
        await _eventStoreService.SaveEventAsync(todoCreatedEvent);
        
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ListTodos()
    {
        // Retrieve and return the items from your data store (e.g., database)
        // Implementation of retrieving from the database is beyond the scope of this example.
        // You may use an ORM or a simple in-memory list for demonstration purposes.

        // For demonstration purposes, assume we have a list of  items.
        var todoItems = new List<TodoItem>
        {
            new TodoItem { Id = Guid.NewGuid(), Title = "Sample TODO 1", IsCompleted = false },
            new TodoItem { Id = Guid.NewGuid(), Title = "Sample TODO 2", IsCompleted = true }
        };

        return Ok(todoItems);
    }
}
