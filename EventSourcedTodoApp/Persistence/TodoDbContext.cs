using EventSourcedTodoApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EventSourcedTodoApp.Persistence;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }
    public DbSet<TodoItem> TodoItems { get; set; }
}