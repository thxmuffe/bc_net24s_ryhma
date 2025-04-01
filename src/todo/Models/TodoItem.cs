namespace Todo.Models;

public class TodoItem
{
    public long TodoItemId { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}