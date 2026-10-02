using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;

namespace ProductionManagement.Wpf.Services;

// 조회된 작업지시 목록을 CSV 파일로 저장
public sealed class WorkOrderCsvService {
    public async Task ExportAsync(
        string filePath,
        IReadOnlyCollection<WorkOrder> workOrders) {
        ArgumentNullException.ThrowIfNull(workOrders);

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("저장할 파일 경로를 선택해 주세요.");

        if (workOrders.Count == 0)
            throw new ArgumentException("저장할 작업지시가 없습니다.");

        var csv = new StringBuilder();

        csv.AppendLine(
            "작업지시 번호,제품 코드,제품명,생산 예정일,"
            + "목표 수량,양품 합계,불량 합계,달성률(%),단위,상태,비고");

        foreach (var workOrder in workOrders) {
            string statusName = workOrder.Status switch {
                "Waiting" => "대기",
                "InProgress" => "진행 중",
                "Completed" => "완료",
                _ => workOrder.Status
            };

            string[] values =
            {
                Escape(workOrder.WorkOrderNo),
                Escape(workOrder.ProductCode),
                Escape(workOrder.ProductName),

                workOrder.PlannedDate.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture),

                workOrder.TargetQuantity.ToString(
                    CultureInfo.InvariantCulture),

                workOrder.TotalGoodQuantity.ToString(
                    CultureInfo.InvariantCulture),

                workOrder.TotalDefectQuantity.ToString(
                    CultureInfo.InvariantCulture),

                workOrder.AchievementRate.ToString(
                    "F2", CultureInfo.InvariantCulture),

                Escape(workOrder.Unit),
                Escape(statusName),
                Escape(workOrder.Memo)
            };

            csv.AppendLine(string.Join(",", values));
        }

        // 한글을 인식하기 쉽도록 UTF-8 BOM을 포함해 저장
        await File.WriteAllTextAsync(
            filePath,
            csv.ToString(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    // 쉼표·줄바꿈·큰따옴표가 있어도 하나의 셀로 저장
    private static string Escape(string? value) {
        value ??= string.Empty;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}