namespace MVVM.Shared.Dtos;

public sealed record TodoItemUpdateDto
{
    public string Title { get; init; } = string.Empty;
    public bool IsDone { get; init; }
}
