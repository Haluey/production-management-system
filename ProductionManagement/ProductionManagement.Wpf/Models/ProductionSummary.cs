namespace ProductionManagement.Wpf.Models;

// 생산 현황 화면에 표시할 전체 작업지시 요약
public sealed class ProductionSummary {
    // 전체 작업지시 건수
    public int WorkOrderCount { get; set; }

    // 작업지시 목표 수량의 합계
    public long TotalTargetQuantity { get; set; }

    // 등록된 양품 수량의 합계
    public long TotalGoodQuantity { get; set; }

    // 등록된 불량 수량의 합계
    public long TotalDefectQuantity { get; set; }

    // 전체 양품 합계 ÷ 전체 목표 합계 × 100
    // 작업지시별 달성률의 단순 평균이 아님
    public decimal AchievementRate { get; set; }
}