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

        var connectionFactory = new SqlConnectionFactory();

        var repository = new WorkOrderRepository(connectionFactory);
        var service = new WorkOrderService(repository);

        var productRepository = new ProductRepository(connectionFactory);
        var productService = new ProductService(productRepository);

        _viewModel = new WorkOrderListViewModel(service, productService);
        DataContext = _viewModel;
    }

    // 처리 중 입력·선택·중복 실행 방지
    private void SetBusy(bool isBusy) {
        _isBusy = isBusy;

        RefreshButton.IsEnabled = !isBusy;
        WorkOrderInputPanel.IsEnabled = !isBusy;
        WorkOrderGrid.IsEnabled = !isBusy;
        StartButton.IsEnabled = !isBusy;
        CompleteButton.IsEnabled = !isBusy;
        FilterPanel.IsEnabled = !isBusy;
        ResetFiltersButton.IsEnabled = !isBusy;
    }

    private async void RefreshWorkOrders_Click(
        object sender,
        RoutedEventArgs e) {
        if (_isBusy)
            return;

        SetBusy(true);

        try {
            await _viewModel.LoadAsync();
        }
        catch (ArgumentException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "조회 조건 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"작업지시 조회 실패\n{ex.Message}",
                "조회 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            SetBusy(false);
        }
    }

    private async void CreateWorkOrder_Click(
        object sender,
        RoutedEventArgs e) {
        if (_isBusy)
            return;

        SetBusy(true);

        try {
            await _viewModel.CreateAsync();

            // 저장 성공 후 목록 새로고침
            try {
                await _viewModel.LoadAsync();
            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show(
                    $"작업지시는 등록됐지만 목록 갱신에 실패했습니다.\n"
                    + $"작업지시 조회 버튼을 눌러 주세요.\n{ex.Message}",
                    "목록 갱신 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            System.Windows.MessageBox.Show(
                "작업지시가 등록됐습니다.",
                "등록 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (ArgumentException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "입력 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (InvalidOperationException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "등록 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"등록 처리 중 오류가 발생했습니다.\n"
                + $"작업지시 조회로 저장 여부를 확인해 주세요.\n{ex.Message}",
                "등록 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            SetBusy(false);
        }
    }

    private async void StartWorkOrder_Click(
        object sender,
        RoutedEventArgs e) {
        if (_isBusy)
            return;

        var workOrder = _viewModel.SelectedWorkOrder;

        if (workOrder is null) {
            System.Windows.MessageBox.Show(
                "시작할 작업지시를 선택해 주세요.",
                "선택 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var answer = System.Windows.MessageBox.Show(
            $"{workOrder.WorkOrderNo}\n"
            + $"{workOrder.ProductName}\n"
            + "이 작업을 시작하시겠습니까?",
            "작업 시작",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (answer != MessageBoxResult.Yes)
            return;

        SetBusy(true);

        try {
            await _viewModel.StartAsync();

            // 시작 성공 후 최신 상태 조회
            try {
                await _viewModel.LoadAsync();
            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show(
                    $"작업은 시작됐지만 목록 갱신에 실패했습니다.\n"
                    + $"작업지시 조회 버튼을 눌러 주세요.\n{ex.Message}",
                    "목록 갱신 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            System.Windows.MessageBox.Show(
                "작업이 시작됐습니다.",
                "시작 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (ArgumentException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "입력 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (InvalidOperationException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "시작 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"작업 시작 중 오류가 발생했습니다.\n"
                + $"작업지시 조회로 상태를 확인해 주세요.\n{ex.Message}",
                "시작 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            SetBusy(false);
        }
    }

    private async void CompleteWorkOrder_Click(
        object sender,
        RoutedEventArgs e) {
        if (_isBusy)
            return;

        var workOrder = _viewModel.SelectedWorkOrder;

        if (workOrder is null) {
            System.Windows.MessageBox.Show(
                "완료할 작업지시를 선택해 주세요.",
                "선택 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        // 화면에 표시된 상태를 먼저 확인
        // 실제 저장 시점의 상태는 DB에서도 다시 검사
        if (workOrder.Status != "InProgress") {
            System.Windows.MessageBox.Show(
                "진행 중인 작업지시만 완료할 수 있습니다.",
                "상태 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var answer = System.Windows.MessageBox.Show(
            $"{workOrder.WorkOrderNo}\n"
            + $"{workOrder.ProductName}\n\n"
            + "이 작업을 완료하시겠습니까?\n"
            + "목표 달성률과 관계없이 완료 처리되며, "
            + "완료 후에는 생산실적을 추가할 수 없습니다.",
            "작업 완료 확인",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        if (answer != MessageBoxResult.Yes)
            return;

        SetBusy(true);

        try {
            await _viewModel.CompleteAsync();

            // 완료 성공 후 최신 상태 조회
            try {
                await _viewModel.LoadAsync();
            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show(
                    "작업은 완료됐지만 목록 갱신에 실패했습니다.\n"
                    + $"작업지시 조회 버튼을 눌러 주세요.\n{ex.Message}",
                    "목록 갱신 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            System.Windows.MessageBox.Show(
                "작업이 완료됐습니다.",
                "완료 처리",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (ArgumentException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "입력 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (InvalidOperationException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "완료 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                "작업 완료 중 오류가 발생했습니다.\n"
                + $"작업지시 조회로 상태를 확인해 주세요.\n{ex.Message}",
                "완료 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            SetBusy(false);
        }
    }

    // 조건을 초기화한 뒤 전체 작업지시 조회
    private void ResetFilters_Click(
        object sender,
        RoutedEventArgs e) {
        if (_isBusy)
            return;

        _viewModel.ResetFilters();
        RefreshWorkOrders_Click(sender, e);
    }
}