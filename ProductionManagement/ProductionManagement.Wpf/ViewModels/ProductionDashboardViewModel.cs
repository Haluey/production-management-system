using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;
using ProductionManagement.Wpf.Services;

namespace ProductionManagement.Wpf.ViewModels;

public sealed class ProductionDashboardViewModel : ViewModelBase {
    private readonly WorkOrderService _workOrderService;
    private readonly ProductionSummaryService _summaryService;

    private ProductionSummary _summary = new();

    // 차트와 목록에 사용할 작업지시
    public ObservableCollection<WorkOrder> WorkOrders { get; } = new();

    // 현황 카드에 표시할 요약값
    public ProductionSummary Summary {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    public ProductionDashboardViewModel(
        WorkOrderService workOrderService,
        ProductionSummaryService summaryService) {
        _workOrderService = workOrderService
            ?? throw new ArgumentNullException(nameof(workOrderService));

        _summaryService = summaryService
            ?? throw new ArgumentNullException(nameof(summaryService));
    }

    // 전체 작업지시를 조회한 뒤 같은 데이터로 요약 계산
    public async Task LoadAsync() {
        var workOrders = await _workOrderService.GetAllAsync();
        var summary = _summaryService.Calculate(workOrders);

        // 조회와 계산이 성공한 뒤 화면 데이터 갱신
        WorkOrders.Clear();

        foreach (var workOrder in workOrders) {
            WorkOrders.Add(workOrder);
        }

        Summary = summary;
    }
}