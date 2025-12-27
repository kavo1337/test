using System.Net.Http;
using System.Net.Http.Json;
using MVVM.Shared.Dtos;

namespace MVVM.WEB.Services;

public sealed class TodoApiService
{
    private readonly HttpClient _httpClient;

    public TodoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<TodoItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/todos", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var items = await response.Content.ReadFromJsonAsync<List<TodoItemDto>>(cancellationToken: cancellationToken);
        return items ?? new List<TodoItemDto>();
    }

    public async Task<TodoItemDto?> CreateAsync(string title, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/todos",
            new TodoItemCreateDto { Title = title },
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<TodoItemDto>(cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(int id, TodoItemUpdateDto input, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/todos{id}", input, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/todos{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = $"Ошибка : {(int)response.StatusCode} {response.ReasonPhrase}";
        }

        throw new ApiException(message);
    }
}
