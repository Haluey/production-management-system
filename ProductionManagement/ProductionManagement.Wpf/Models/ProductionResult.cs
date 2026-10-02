using System;

namespace ProductionManagement.Wpf.Models;

// 작업지시에 등록된 생산실적 한 건
public sealed class ProductionResult {
    public int ProductionResultId { get; set; }

    // 연결된 작업지시의 내부 번호
    public int WorkOrderId { get; set; }

    // 생산일: 시간 없이 날짜만 저장
    public DateOnly ProductionDate { get; set; }

    // 정상적으로 생산된 수량
    public int GoodQuantity { get; set; }

    // 불량으로 판정된 수량
    public int DefectQuantity { get; set; }

    // 불량 사유 등 선택 입력 비고
    public string? Memo { get; set; }

    // 실적을 등록한 시각(UTC)
    public DateTime CreatedAt { get; set; }
}