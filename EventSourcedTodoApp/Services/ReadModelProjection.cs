using System.Text.Json;
using EventSourcedTodoApp.Events;
using EventSourcedTodoApp.Models;
using EventSourcedTodoApp.Persistence;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;

namespace EventSourcedTodoApp.Services;

public class ReadModelProjection : BackgroundService
{
    private readonly ILogger<ReadModelProjection> _logger;
    private readonly IEventStore _eventStore;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ReadModelProjection(
        ILogger<ReadModelProjection> logger,
        IEventStore eventStore,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _eventStore = eventStore;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background service started.");

        await _eventStore.SubscribeToAllStreams(HandleEventAsync, stoppingToken);

        _logger.LogInformation("Background service stopped.");
    }


    private async Task HandleEventAsync(IEvent @event)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        switch (@event)
        {
            case TodoCreatedEvent todoCreatedEvent:
                dbContext.TodoItems.Add(new TodoItem
                {
                    Id = todoCreatedEvent.Id,
                    Title = todoCreatedEvent.Title
                });
                break;
            case TodoStatusUpdatedEvent todoStatusUpdatedEvent:
                var todoItem = await dbContext.TodoItems.FindAsync(todoStatusUpdatedEvent.Id);
                if (todoItem != null)
                {
                    todoItem.IsCompleted = todoStatusUpdatedEvent.IsCompleted;
                }
                break;
        }

        await dbContext.SaveChangesAsync();
    }
}