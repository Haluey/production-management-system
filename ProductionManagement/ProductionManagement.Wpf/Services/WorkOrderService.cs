using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Models;
using Microsoft.Data.SqlClient;

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

    // 입력값을 검사한 뒤 작업지시 등록
    public async Task<int> CreateAsync(
        string workOrderNo,
        int productId,
        DateOnly plannedDate,
        int targetQuantity,
        string? memo) {
        // 앞뒤 공백 제거
        workOrderNo = (workOrderNo ?? string.Empty).Trim();
        memo = memo?.Trim();

        if (string.IsNullOrWhiteSpace(workOrderNo))
            throw new ArgumentException("작업지시 번호를 입력해 주세요.");

        if (workOrderNo.Length > 30)
            throw new ArgumentException(
                "작업지시 번호는 30자 이하로 입력해 주세요.");

        if (productId <= 0)
            throw new ArgumentException("생산할 제품을 선택해 주세요.");

        if (targetQuantity <= 0)
            throw new ArgumentException("목표 수량은 1 이상이어야 합니다.");

        if (memo is not null && memo.Length > 500)
            throw new ArgumentException("비고는 500자 이하로 입력해 주세요.");

        // 비고를 입력하지 않았다면 NULL로 저장
        if (string.IsNullOrWhiteSpace(memo))
            memo = null;

        try {
            int? workOrderId = await _workOrderRepository.CreateAsync(
                workOrderNo,
                productId,
                plannedDate,
                targetQuantity,
                memo);

            // 저장 시점에 제품이 없거나 비활성인 경우
            if (workOrderId is null) {
                throw new InvalidOperationException(
                    "선택한 제품이 없거나 비활성 상태입니다. "
                    + "제품 목록을 다시 불러온 뒤 선택해 주세요.");
            }

            return workOrderId.Value;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627) {
            throw new InvalidOperationException(
                "이미 등록된 작업지시 번호입니다.",
                ex);
        }
    }
}