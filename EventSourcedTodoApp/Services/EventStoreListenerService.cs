using System.Text.Json;
using EventSourcedTodoApp.Events;
using EventSourcedTodoApp.Models;
using EventSourcedTodoApp.Persistence;
using EventStore.Client;

namespace EventSourcedTodoApp.Services;

public class EventStoreListenerService : BackgroundService
{
    private readonly ILogger<EventStoreListenerService> _logger;
    private readonly EventStoreClient _eventStoreClient;
    private readonly EventStoreService _eventStoreService;
    private readonly string _streamName = "todo-events";
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventStoreListenerService(
        ILogger<EventStoreListenerService> logger,
        EventStoreClient eventStoreClient,
        EventStoreService eventStoreService,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _eventStoreClient = eventStoreClient;
        _eventStoreService = eventStoreService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background service started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
                    var eventStoreClient = scope.ServiceProvider.GetRequiredService<EventStoreClient>();

                    var streamSubscription = eventStoreClient.SubscribeToStreamAsync(
                        _streamName,
                        FromStream.Start, 
                        async (subscription, resolvedEvent, cancellationToken) =>
                        {
                            await HandleEventAsync(subscription, resolvedEvent, dbContext, cancellationToken);
                        },
                        resolveLinkTos: false,
                        cancellationToken: stoppingToken);

                    var tcs = new TaskCompletionSource<bool>();

                    // Wait for either streamSubscription to complete or stoppingToken to be canceled
                    using (var registration = stoppingToken.Register(() => tcs.TrySetResult(true)))
                    {
                        await Task.WhenAny(streamSubscription, tcs.Task);
                    }

                    streamSubscription.Dispose();
                }
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore the TaskCanceledException that occurs when the application is shutting down
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred in the background service.");
        }

        _logger.LogInformation("Background service stopped.");
    }

    
    private async Task HandleEventAsync(
        StreamSubscription subscription, 
        ResolvedEvent resolvedEvent, 
        TodoDbContext dbContext, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Deserialize the event payload
            var eventType = Type.GetType(resolvedEvent.Event.EventType);
            var eventPayload = JsonSerializer.Deserialize(
                resolvedEvent.Event.Data.Span,
                eventType);

            // Check if the event is of type TodoCreatedEvent
            if (eventPayload is TodoCreatedEvent todoCreatedEvent)
            {
                // Create a new entity using the payload from the event
                var todoItem = new TodoItem
                {
                    Id = todoCreatedEvent.Id,
                    Title = todoCreatedEvent.Title
                };

                // Add the entity to the EF Core database
                dbContext.TodoItems.Add(todoItem);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation($"Todo with ID {todoItem.Id} added to the database.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling event.");
        }
    }
}