using System;
using System.Threading.Tasks;
using System.Windows;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Services;
using ProductionManagement.Wpf.ViewModels;

namespace ProductionManagement.Wpf.Views;

public partial class ProductionResultView : System.Windows.Controls.UserControl {
    private readonly ProductionResultViewModel _viewModel;
    private bool _isBusy;
    private bool _hasLoaded;

    public ProductionResultView() {
        InitializeComponent();

        var connectionFactory = new SqlConnectionFactory();

        var workOrderRepository = new WorkOrderRepository(connectionFactory);
        var workOrderService = new WorkOrderService(workOrderRepository);

        var resultRepository = new ProductionResultRepository(connectionFactory);
        var resultService = new ProductionResultService(resultRepository);

        _viewModel = new ProductionResultViewModel(
            workOrderService,
            resultService);

        DataContext = _viewModel;
    }

    private async void LoadWorkOrders_Click(
    object sender,
    RoutedEventArgs e) {
        await ExecuteAsync(async () =>
        {
            await _viewModel.LoadWorkOrdersAsync();

            // 최초 조회 성공 여부 기록
            _hasLoaded = true;
        }, "작업지시 조회");
    }

    private async void LoadResults_Click(
        object sender,
        RoutedEventArgs e) {
        await ExecuteAsync(
            () => _viewModel.LoadResultsAsync(),
            "실적 조회");
    }

    private async void CreateResult_Click(
        object sender,
        RoutedEventArgs e) {
        await ExecuteAsync(async () => {
            await _viewModel.CreateAsync();

            // 저장 성공 후 해당 작업의 실적 목록 갱신
            try {
                await _viewModel.LoadResultsAsync();
            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show(
                    $"실적은 등록됐지만 목록 갱신에 실패했습니다.\n"
                    + $"실적 조회 버튼을 눌러 주세요.\n{ex.Message}",
                    "목록 갱신 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            System.Windows.MessageBox.Show(
                "생산실적이 등록됐습니다.",
                "등록 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }, "실적 등록");
    }

    // 각 버튼의 중복 실행 방지와 오류 처리를 공통으로 수행
    private async Task ExecuteAsync(
        Func<Task> action,
        string title) {
        if (_isBusy)
            return;

        _isBusy = true;
        ContentGrid.IsEnabled = false;

        try {
            await action();
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
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"처리 중 오류가 발생했습니다.\n"
                + $"등록을 시도했다면 실적 조회로 저장 여부를 확인해 주세요.\n"
                + ex.Message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            _isBusy = false;
            ContentGrid.IsEnabled = true;
        }
    }

    // 탭을 처음 열 때 작업지시 목록 자동 조회
    private void ProductionResultView_Loaded(
        object sender,
        RoutedEventArgs e) {
        if (_hasLoaded || _isBusy)
            return;

        LoadWorkOrders_Click(sender, e);
    }

    // 작업지시를 선택하면 해당 생산실적 자동 조회
    private async void WorkOrderSelection_Changed(
        object sender,
        System.Windows.Controls.SelectionChangedEventArgs e) {
        if (_isBusy || e.AddedItems.Count == 0)
            return;

        if (e.AddedItems[0]
            is not ProductionManagement.Wpf.Models.WorkOrder workOrder)
            return;

        // 선택 이벤트의 작업지시를 조회 대상으로 확정
        _viewModel.SelectedWorkOrder = workOrder;

        await ExecuteAsync(
            () => _viewModel.LoadResultsAsync(),
            "실적 조회");
    }
}