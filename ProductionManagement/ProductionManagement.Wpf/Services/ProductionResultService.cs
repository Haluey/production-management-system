using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Models;

namespace ProductionManagement.Wpf.Services;

// 생산실적 관련 기능과 입력 검사
public sealed class ProductionResultService {
    private readonly ProductionResultRepository _repository;

    public ProductionResultService(
        ProductionResultRepository repository) {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    // 지정한 작업지시의 생산실적 조회
    public Task<List<ProductionResult>> GetByWorkOrderAsync(
        int workOrderId) {
        if (workOrderId <= 0)
            throw new ArgumentException("작업지시를 선택해 주세요.");

        return _repository.GetByWorkOrderAsync(workOrderId);
    }

    // 입력값을 검사한 뒤 생산실적 등록
    public async Task<int> CreateAsync(
        int workOrderId,
        DateOnly productionDate,
        int goodQuantity,
        int defectQuantity,
        string? memo) {
        if (workOrderId <= 0)
            throw new ArgumentException("작업지시를 선택해 주세요.");

        if (goodQuantity < 0)
            throw new ArgumentException("양품 수량은 0 이상이어야 합니다.");

        if (defectQuantity < 0)
            throw new ArgumentException("불량 수량은 0 이상이어야 합니다.");

        if (goodQuantity == 0 && defectQuantity == 0) {
            throw new ArgumentException(
                "양품 또는 불량 수량 중 하나는 1 이상이어야 합니다.");
        }

        memo = memo?.Trim();

        if (memo is not null && memo.Length > 500)
            throw new ArgumentException("비고는 500자 이하로 입력해 주세요.");

        if (string.IsNullOrWhiteSpace(memo))
            memo = null;

        int? resultId = await _repository.CreateAsync(
            workOrderId,
            productionDate,
            goodQuantity,
            defectQuantity,
            memo);

        // 저장 시점의 작업지시 상태 확인 결과
        if (resultId is null) {
            throw new InvalidOperationException(
                "진행 중인 작업지시에만 실적을 등록할 수 있습니다. "
                + "작업지시 목록을 다시 조회해 주세요.");
        }

        return resultId.Value;
    }
}