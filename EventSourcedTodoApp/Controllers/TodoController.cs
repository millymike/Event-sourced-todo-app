using EventSourcedTodoApp.Events;
using EventSourcedTodoApp.Models;
using EventSourcedTodoApp.Persistence;
using EventSourcedTodoApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventSourcedTodoApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly Services.IEventStore _eventStore;
    private readonly TodoDbContext _dbContext;

    public TodoController(Services.IEventStore eventStore, TodoDbContext dbContext)
    {
        _eventStore = eventStore;
        _dbContext = dbContext;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos()
    {
        return await _dbContext.TodoItems.ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> CreateTodo(TodoItem todoItem)
    {
        // Save the item creation event to EventStoreDB
        var todoCreatedEvent = new TodoCreatedEvent { Id = todoItem.Id, Title = todoItem.Title };

        await _eventStore.WriteEvents(
            $"todo-{todoItem.Id}",
            new[] { todoCreatedEvent },
            HttpContext.RequestAborted);

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTodo(TodoItem todoItem)
    {
        var todoUpdatedEvent = new TodoStatusUpdatedEvent { Id = todoItem.Id, IsCompleted = todoItem.IsCompleted };

        await _eventStore.WriteEvents(
            $"todo-{todoItem.Id}",
            new[] { todoUpdatedEvent },
            HttpContext.RequestAborted);

        return Ok();
    }
    
}