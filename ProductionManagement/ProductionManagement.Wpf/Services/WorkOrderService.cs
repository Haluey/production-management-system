using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Models;

namespace ProductionManagement.Wpf.Services;

// 작업지시 관련 기능과 업무 규칙 처리
public sealed class WorkOrderService {
    private readonly WorkOrderRepository _workOrderRepository;

    public WorkOrderService(WorkOrderRepository workOrderRepository) {
        _workOrderRepository = workOrderRepository
            ?? throw new ArgumentNullException(nameof(workOrderRepository));
    }

    // 전체 작업지시 목록 조회
    public Task<List<WorkOrder>> GetAllAsync() {
        return _workOrderRepository.GetAllAsync();
    }
}