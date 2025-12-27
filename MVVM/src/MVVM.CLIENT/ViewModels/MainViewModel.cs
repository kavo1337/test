using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using MVVM.Shared.Dtos;
using MVVM.WEB.Services;

namespace MVVM.WEB.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly TodoApiService _apiService;
    private string _newTitle = string.Empty;
    private bool _isBusy;
    private string? _errorMessage;
    private int _totalCount;
    private int _doneCount;
    private int _remainingCount;

    public MainViewModel(TodoApiService apiService)
    {
        _apiService = apiService;

        LoadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        AddCommand = new AsyncRelayCommand(AddAsync, () => !IsBusy && !string.IsNullOrWhiteSpace(NewTitle));
        ToggleCommand = new AsyncRelayCommand<TodoItemDto>(ToggleAsync, item => !IsBusy && item is not null);
        DeleteCommand = new AsyncRelayCommand<TodoItemDto>(DeleteAsync, item => !IsBusy && item is not null);
    }

    public ObservableCollection<TodoItemDto> Items { get; } = new();

    public string NewTitle
    {
        get => _newTitle;
        set
        {
            if (SetProperty(ref _newTitle, value))
            {
                AddCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LoadCommand.RaiseCanExecuteChanged();
                AddCommand.RaiseCanExecuteChanged();
                ToggleCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public int DoneCount
    {
        get => _doneCount;
        private set => SetProperty(ref _doneCount, value);
    }

    public int RemainingCount
    {
        get => _remainingCount;
        private set => SetProperty(ref _remainingCount, value);
    }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddCommand { get; }
    public AsyncRelayCommand<TodoItemDto> ToggleCommand { get; }
    public AsyncRelayCommand<TodoItemDto> DeleteCommand { get; }

    public Task LoadAsync() => RunBusyAsync(LoadItemsAsync);

    private async Task AddAsync()
    {
        var title = NewTitle.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            ErrorMessage = "Введите название задачи.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _apiService.CreateAsync(title);
            NewTitle = string.Empty;
            await LoadItemsAsync();
        });
    }

    private Task ToggleAsync(TodoItemDto? item)
    {
        if (item is null)
        {
            return Task.CompletedTask;
        }

        return RunBusyAsync(async () =>
        {
            var update = new TodoItemUpdateDto
            {
                Title = item.Title,
                IsDone = !item.IsDone
            };

            await _apiService.UpdateAsync(item.Id, update);
            await LoadItemsAsync();
        });
    }

    private Task DeleteAsync(TodoItemDto? item)
    {
        if (item is null)
        {
            return Task.CompletedTask;
        }

        return RunBusyAsync(async () =>
        {
            await _apiService.DeleteAsync(item.Id);
            await LoadItemsAsync();
        });
    }

    private async Task LoadItemsAsync()
    {
        var items = await _apiService.GetAllAsync();
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }

        UpdateCounts();
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await action();
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Не удалось подключиться к серверу API.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "Запрос к серверу был отменен.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateCounts()
    {
        TotalCount = Items.Count;
        DoneCount = Items.Count(item => item.IsDone);
        RemainingCount = Math.Max(0, TotalCount - DoneCount);
    }
}
