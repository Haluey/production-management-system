using System;

namespace ProductionManagement.Wpf.Models;

// 작업지시 한 건을 담는 클래스
public sealed class WorkOrder {
    public int WorkOrderId { get; set; }

    public string WorkOrderNo { get; set; } = string.Empty;

    // 생산할 제품
    public int ProductId { get; set; }

    // Products 테이블과 JOIN해서 가져올 표시용 정보
    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    // 생산 예정일: 시간 없이 날짜만 사용
    public DateOnly PlannedDate { get; set; }

    public int TargetQuantity { get; set; }

    // Waiting: 대기 / InProgress: 진행 중 / Completed: 완료
    public string Status { get; set; } = "Waiting";

    // 시작 전·완료 전에는 값이 없으므로 nullable 사용
    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Memo { get; set; }

    // 생성·시작·완료 시각은 UTC 기준
    public DateTime CreatedAt { get; set; }
}