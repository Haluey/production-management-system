using System;
using System.Collections.Generic;
using System.Linq;
using ProductionManagement.Wpf.Models;

namespace ProductionManagement.Wpf.Services;

// 작업지시 목록을 기준으로 생산 현황 요약 계산
public sealed class ProductionSummaryService {
    public ProductionSummary Calculate(
        IReadOnlyCollection<WorkOrder> workOrders) {
        ArgumentNullException.ThrowIfNull(workOrders);

        // 목표 수량도 long으로 변환한 뒤 합산
        long totalTargetQuantity =
            workOrders.Sum(w => (long)w.TargetQuantity);

        long totalGoodQuantity =
            workOrders.Sum(w => w.TotalGoodQuantity);

        long totalDefectQuantity =
            workOrders.Sum(w => w.TotalDefectQuantity);

        // 작업지시가 없으면 달성률은 0
        // 소수점 둘째 자리까지 반올림
        decimal achievementRate = totalTargetQuantity == 0
            ? 0m
            : Math.Round(
                totalGoodQuantity * 100m / totalTargetQuantity,
                2,
                MidpointRounding.AwayFromZero);

        return new ProductionSummary {
            WorkOrderCount = workOrders.Count,
            TotalTargetQuantity = totalTargetQuantity,
            TotalGoodQuantity = totalGoodQuantity,
            TotalDefectQuantity = totalDefectQuantity,
            AchievementRate = achievementRate
        };
    }
}