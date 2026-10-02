using System;
using System.Windows;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Services;
using ProductionManagement.Wpf.ViewModels;

namespace ProductionManagement.Wpf;

public partial class MainWindow : Window {
    private readonly ProductListViewModel _viewModel;

    public MainWindow() {
        InitializeComponent();

        // DB 연결 → Repository → Service → ViewModel 구성
        var connectionFactory = new SqlConnectionFactory();
        var productRepository = new ProductRepository(connectionFactory);
        var productService = new ProductService(productRepository);

        _viewModel = new ProductListViewModel(productService);

        // XAML의 Binding이 사용할 객체 지정
        DataContext = _viewModel;
    }

    private async void RefreshProducts_Click(
        object sender,
        RoutedEventArgs e) {
        // 조회 중에는 버튼을 비활성화해 중복 클릭 방지
        RefreshButton.IsEnabled = false;

        try {
            await _viewModel.LoadAsync();
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"제품 조회 실패\n{ex.Message}",
                "조회 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            // 성공하거나 실패해도 버튼 다시 활성화
            RefreshButton.IsEnabled = true;
        }
    }
}