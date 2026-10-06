using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;
using ProductionManagement.Wpf.Services;

namespace ProductionManagement.Wpf.ViewModels;

public sealed class WorkOrderListViewModel : ViewModelBase {
    private readonly WorkOrderService _workOrderService;
    private readonly ProductService _productService;

    private WorkOrder? _selectedWorkOrder;
    private Product? _selectedProduct;
    private string _workOrderNo = string.Empty;
    private DateTime? _plannedDate = DateTime.Today;
    private string _targetQuantityText = string.Empty;
    private string _memo = string.Empty;

    // 조회 조건: 날짜를 선택하지 않으면 해당 범위를 제한하지 않음
    private DateTime? _filterStartDate;
    private DateTime? _filterEndDate;

    // All: 전체 / Waiting: 대기 / InProgress: 진행 중 / Completed: 완료
    private string _filterStatus = "All";

    // 작업지시 번호·제품 코드·제품명 검색
    private string _searchKeyword = string.Empty;

    // 조회한 작업지시 목록
    public ObservableCollection<WorkOrder> WorkOrders { get; } = new();

    // 등록 시 선택할 활성 제품 목록
    public ObservableCollection<Product> ActiveProducts { get; } = new();

    // 표에서 선택한 작업지시
    public WorkOrder? SelectedWorkOrder {
        get => _selectedWorkOrder;
        set => SetProperty(ref _selectedWorkOrder, value);
    }

    // 새 작업지시에 사용할 제품
    public Product? SelectedProduct {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    public string WorkOrderNo {
        get => _workOrderNo;
        set => SetProperty(ref _workOrderNo, value);
    }

    // WPF 날짜 선택 컨트롤에 연결할 값
    public DateTime? PlannedDate {
        get => _plannedDate;
        set => SetProperty(ref _plannedDate, value);
    }

    // 숫자가 아닌 입력도 직접 검사할 수 있도록 문자열로 보관
    public string TargetQuantityText {
        get => _targetQuantityText;
        set => SetProperty(ref _targetQuantityText, value);
    }

    public string Memo {
        get => _memo;
        set => SetProperty(ref _memo, value);
    }

    // 생산 예정일 조회 시작일
    public DateTime? FilterStartDate {
        get => _filterStartDate;
        set => SetProperty(ref _filterStartDate, value);
    }

    // 생산 예정일 조회 종료일
    public DateTime? FilterEndDate {
        get => _filterEndDate;
        set => SetProperty(ref _filterEndDate, value);
    }

    // 조회할 작업 상태
    public string FilterStatus {
        get => _filterStatus;
        set => SetProperty(ref _filterStatus, value);
    }

    // 검색어
    public string SearchKeyword {
        get => _searchKeyword;
        set => SetProperty(ref _searchKeyword, value);
    }

    public WorkOrderListViewModel(
        WorkOrderService workOrderService,
        ProductService productService) {
        _workOrderService = workOrderService
            ?? throw new ArgumentNullException(nameof(workOrderService));

        _productService = productService
            ?? throw new ArgumentNullException(nameof(productService));
    }

    // 작업지시와 활성 제품 목록 조회
    // 조회 조건에 맞는 작업지시 목록만 갱신
    public async Task LoadAsync() {
        DateOnly? startDate = FilterStartDate.HasValue
            ? DateOnly.FromDateTime(FilterStartDate.Value)
            : null;

        DateOnly? endDate = FilterEndDate.HasValue
            ? DateOnly.FromDateTime(FilterEndDate.Value)
            : null;

        var workOrders = await _workOrderService.GetAllAsync(
            startDate,
            endDate,
            FilterStatus,
            SearchKeyword);

        SelectedWorkOrder = null;
        WorkOrders.Clear();

        foreach (var workOrder in workOrders) {
            WorkOrders.Add(workOrder);
        }
    }

    // 등록용 제품 목록 갱신
    public async Task LoadActiveProductsAsync() {
        // 현재 선택한 제품의 ID를 기억
        int? selectedProductId = SelectedProduct?.ProductId;

        // 조회에 성공한 뒤 화면 목록을 변경
        var products = await _productService.GetActiveAsync();

        SelectedProduct = null;
        ActiveProducts.Clear();

        foreach (var product in products) {
            ActiveProducts.Add(product);

            // 같은 제품이 여전히 활성 상태라면 선택 유지
            // 제품명 등이 변경됐으면 새 정보로 반영
            if (product.ProductId == selectedProductId) {
                SelectedProduct = product;
            }
        }

        // 선택했던 제품이 비활성화됐다면 선택은 해제됨
    }

    // 새 작업지시 등록
    public async Task<int> CreateAsync() {
        var product = SelectedProduct;
        var plannedDate = PlannedDate;

        if (product is null)
            throw new ArgumentException("생산할 제품을 선택해 주세요.");

        if (plannedDate is null)
            throw new ArgumentException("생산 예정일을 선택해 주세요.");

        if (!int.TryParse(TargetQuantityText, out int targetQuantity)
            || targetQuantity <= 0) {
            throw new ArgumentException(
                "목표 수량은 1 이상의 정수로 입력해 주세요.");
        }

        int workOrderId = await _workOrderService.CreateAsync(
            WorkOrderNo,
            product.ProductId,
            DateOnly.FromDateTime(plannedDate.Value),
            targetQuantity,
            Memo);

        // 저장에 성공한 경우에만 입력값 초기화
        WorkOrderNo = string.Empty;
        SelectedProduct = null;
        PlannedDate = DateTime.Today;
        TargetQuantityText = string.Empty;
        Memo = string.Empty;

        return workOrderId;
    }

    // 표에서 선택한 작업지시 시작
    public async Task StartAsync() {
        var workOrder = SelectedWorkOrder;

        if (workOrder is null)
            throw new ArgumentException("시작할 작업지시를 선택해 주세요.");

        await _workOrderService.StartAsync(workOrder.WorkOrderId);
    }

    // 표에서 선택한 작업지시 완료
    public async Task CompleteAsync() {
        var workOrder = SelectedWorkOrder;

        if (workOrder is null)
            throw new ArgumentException(
                "완료할 작업지시를 선택해 주세요.");

        await _workOrderService.CompleteAsync(workOrder.WorkOrderId);
    }

    // 조회 조건을 전체 조회 기본값으로 초기화
    public void ResetFilters() {
        FilterStartDate = null;
        FilterEndDate = null;
        FilterStatus = "All";
        SearchKeyword = string.Empty;
    }
}