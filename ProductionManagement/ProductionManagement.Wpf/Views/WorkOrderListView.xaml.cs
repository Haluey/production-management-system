using System;
using System.Windows;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Services;
using ProductionManagement.Wpf.ViewModels;

namespace ProductionManagement.Wpf.Views;

public partial class WorkOrderListView : System.Windows.Controls.UserControl {
    private readonly WorkOrderListViewModel _viewModel;
    private bool _isBusy;

    public WorkOrderListView() {
        InitializeComponent();

        // 작업지시 조회에 필요한 객체 구성
        var connectionFactory = new SqlConnectionFactory();
        var repository = new WorkOrderRepository(connectionFactory);
        var service = new WorkOrderService(repository);

        _viewModel = new WorkOrderListViewModel(service);
        DataContext = _viewModel;
    }

    private async void RefreshWorkOrders_Click(
        object sender,
        RoutedEventArgs e) {
        if (_isBusy)
            return;

        _isBusy = true;
        RefreshButton.IsEnabled = false;
        WorkOrderGrid.IsEnabled = false;

        try {
            await _viewModel.LoadAsync();
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"작업지시 조회 실패\n{ex.Message}",
                "조회 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            _isBusy = false;
            RefreshButton.IsEnabled = true;
            WorkOrderGrid.IsEnabled = true;
        }
    }
}