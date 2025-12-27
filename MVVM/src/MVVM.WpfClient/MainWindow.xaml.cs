using System.Net.Http;
using System.Windows;
using MVVM.WEB.Services;
using MVVM.WEB.ViewModels;

namespace MVVM.WpfClient;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var httpClient = new HttpClient { BaseAddress = ApiOptions.BaseAddress };
        var apiService = new TodoApiService(httpClient);
        DataContext = new MainViewModel(apiService);

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
