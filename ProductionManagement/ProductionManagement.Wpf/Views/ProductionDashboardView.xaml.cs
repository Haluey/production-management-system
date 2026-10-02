using System;
using System.Windows;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Services;
using ProductionManagement.Wpf.ViewModels;

namespace ProductionManagement.Wpf.Views;

public partial class ProductionDashboardView
    : System.Windows.Controls.UserControl {
    private readonly ProductionDashboardViewModel _viewModel;
    private bool _isBusy;

    public ProductionDashboardView() {
        InitializeComponent();

        var connectionFactory = new SqlConnectionFactory();
        var repository = new WorkOrderRepository(connectionFactory);
        var workOrderService = new WorkOrderService(repository);
        var summaryService = new ProductionSummaryService();

        _viewModel = new ProductionDashboardViewModel(
            workOrderService,
            summaryService);

        DataContext = _viewModel;
    }

    private async void RefreshDashboard_Click(
        object sender,
        RoutedEventArgs e) {
        if (_isBusy)
            return;

        _isBusy = true;
        RefreshButton.IsEnabled = false;

        try {
            await _viewModel.LoadAsync();
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"생산 현황 조회에 실패했습니다.\n{ex.Message}",
                "조회 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            _isBusy = false;
            RefreshButton.IsEnabled = true;
        }
    }
}