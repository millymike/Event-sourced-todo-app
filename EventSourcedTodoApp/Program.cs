using EventSourcedTodoApp.Persistence;
using EventSourcedTodoApp.Services;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEventStore>(sp => new EventSourcedTodoApp.Services.EventStore(new EventStoreClient(
    new EventStoreClientSettings
    {
        ConnectivitySettings =
        {
            Address = new Uri(sp.GetRequiredService<IConfiguration>().GetConnectionString("Esdb")!)
        }
    })));
builder.Services.AddDbContextFactory<TodoDbContext>((sp, options) =>
{
    options.UseNpgsql(sp.GetRequiredService<IConfiguration>().GetConnectionString("Postgres")!);
});
builder.Services.AddHostedService<ReadModelProjection>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();