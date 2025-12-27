namespace MVVM.Shared.Dtos;

public sealed record TodoItemDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsDone { get; init; }
}
