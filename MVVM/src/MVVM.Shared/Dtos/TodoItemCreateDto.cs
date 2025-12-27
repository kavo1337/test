namespace MVVM.Shared.Dtos;

public sealed record TodoItemCreateDto
{
    public string Title { get; init; } = string.Empty;
}
