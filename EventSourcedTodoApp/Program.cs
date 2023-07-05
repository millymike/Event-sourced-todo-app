using EventSourcedTodoApp.Models;
using EventSourcedTodoApp.Persistence;
using EventSourcedTodoApp.Services;
using Microsoft.EntityFrameworkCore;
using EventStore.Client;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddSingleton<EventStoreService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetSection("EventStore:ConnectionString").Value;
    return new EventStoreService(connectionString);
});

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();

builder.Services.AddSingleton(appSettings);

builder.Services.AddDbContextFactory<TodoDbContext>(options =>
{
    options.UseNpgsql(appSettings.PostgresDsn);
});


builder.Services.AddSingleton<EventStoreClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<EventStoreClientSettings>>();
    return new EventStoreClient(options.Value);
});


builder.Services.AddHostedService<EventStoreListenerService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();