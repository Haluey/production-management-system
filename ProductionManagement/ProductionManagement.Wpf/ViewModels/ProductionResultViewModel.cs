using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;
using ProductionManagement.Wpf.Services;

namespace ProductionManagement.Wpf.ViewModels;

public sealed class ProductionResultViewModel : ViewModelBase {
    private readonly WorkOrderService _workOrderService;
    private readonly ProductionResultService _resultService;

    private WorkOrder? _selectedWorkOrder;
    private DateTime? _productionDate = DateTime.Today;
    private string _goodQuantityText = "0";
    private string _defectQuantityText = "0";
    private string _memo = string.Empty;

    // 선택할 작업지시 목록
    public ObservableCollection<WorkOrder> WorkOrders { get; } = new();

    // 선택한 작업지시의 생산실적 목록
    public ObservableCollection<ProductionResult> Results { get; } = new();

    public WorkOrder? SelectedWorkOrder {
        get => _selectedWorkOrder;
        set {
            if (SetProperty(ref _selectedWorkOrder, value)) {
                // 다른 작업을 선택하면 이전 작업의 실적 표시 제거
                Results.Clear();

                ProductionDate = DateTime.Today;
                GoodQuantityText = "0";
                DefectQuantityText = "0";
                Memo = string.Empty;
            }
        }
    }

    public DateTime? ProductionDate {
        get => _productionDate;
        set => SetProperty(ref _productionDate, value);
    }

    public string GoodQuantityText {
        get => _goodQuantityText;
        set => SetProperty(ref _goodQuantityText, value);
    }

    public string DefectQuantityText {
        get => _defectQuantityText;
        set => SetProperty(ref _defectQuantityText, value);
    }

    public string Memo {
        get => _memo;
        set => SetProperty(ref _memo, value);
    }

    public ProductionResultViewModel(
        WorkOrderService workOrderService,
        ProductionResultService resultService) {
        _workOrderService = workOrderService
            ?? throw new ArgumentNullException(nameof(workOrderService));

        _resultService = resultService
            ?? throw new ArgumentNullException(nameof(resultService));
    }

    // 작업지시 선택 목록 새로고침
    public async Task LoadWorkOrdersAsync() {
        var workOrders = await _workOrderService.GetAllAsync();

        SelectedWorkOrder = null;
        Results.Clear();
        WorkOrders.Clear();

        foreach (var workOrder in workOrders) {
            WorkOrders.Add(workOrder);
        }
    }

    // 선택한 작업지시의 실적 조회
    public async Task LoadResultsAsync() {
        var workOrder = SelectedWorkOrder;

        if (workOrder is null)
            throw new ArgumentException("작업지시를 선택해 주세요.");

        var results = await _resultService.GetByWorkOrderAsync(
            workOrder.WorkOrderId);

        Results.Clear();

        foreach (var result in results) {
            Results.Add(result);
        }
    }

    // 입력한 생산실적 등록
    public async Task<int> CreateAsync() {
        var workOrder = SelectedWorkOrder;
        var productionDate = ProductionDate;

        if (workOrder is null)
            throw new ArgumentException("작업지시를 선택해 주세요.");

        if (productionDate is null)
            throw new ArgumentException("생산일을 선택해 주세요.");

        if (!int.TryParse(GoodQuantityText, out int goodQuantity)
            || goodQuantity < 0) {
            throw new ArgumentException(
                "양품 수량은 0 이상의 정수로 입력해 주세요.");
        }

        if (!int.TryParse(DefectQuantityText, out int defectQuantity)
            || defectQuantity < 0) {
            throw new ArgumentException(
                "불량 수량은 0 이상의 정수로 입력해 주세요.");
        }

        int resultId = await _resultService.CreateAsync(
            workOrder.WorkOrderId,
            DateOnly.FromDateTime(productionDate.Value),
            goodQuantity,
            defectQuantity,
            Memo);

        // 저장 성공 후 수량과 비고 초기화
        GoodQuantityText = "0";
        DefectQuantityText = "0";
        Memo = string.Empty;

        return resultId;
    }
}