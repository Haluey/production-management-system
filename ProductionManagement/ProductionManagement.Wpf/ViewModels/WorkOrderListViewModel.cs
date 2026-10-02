using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;
using ProductionManagement.Wpf.Services;

namespace ProductionManagement.Wpf.ViewModels;

// 작업지시 목록 화면의 데이터와 조회 기능
public sealed class WorkOrderListViewModel : ViewModelBase {
    private readonly WorkOrderService _workOrderService;
    private WorkOrder? _selectedWorkOrder;

    // 화면에 표시할 작업지시 목록
    public ObservableCollection<WorkOrder> WorkOrders { get; } = new();

    // 표에서 선택한 작업지시
    public WorkOrder? SelectedWorkOrder {
        get => _selectedWorkOrder;
        set => SetProperty(ref _selectedWorkOrder, value);
    }

    public WorkOrderListViewModel(WorkOrderService workOrderService) {
        _workOrderService = workOrderService
            ?? throw new ArgumentNullException(nameof(workOrderService));
    }

    // DB에서 조회한 작업지시를 화면용 목록에 담기
    public async Task LoadAsync() {
        var workOrders = await _workOrderService.GetAllAsync();

        // 목록을 갱신하기 전에 기존 선택 해제
        SelectedWorkOrder = null;
        WorkOrders.Clear();

        foreach (var workOrder in workOrders) {
            WorkOrders.Add(workOrder);
        }
    }
}