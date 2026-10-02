using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;

namespace ProductionManagement.Wpf.Data;

// WorkOrders 테이블의 데이터 조회
public sealed class WorkOrderRepository {
    private readonly SqlConnectionFactory _connectionFactory;

    public WorkOrderRepository(SqlConnectionFactory connectionFactory) {
        _connectionFactory = connectionFactory
            ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    // 작업지시와 연결된 제품 정보를 함께 조회
    public async Task<List<WorkOrder>> GetAllAsync() {
        var workOrders = new List<WorkOrder>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT w.WorkOrderId,
                   w.WorkOrderNo,
                   w.ProductId,
                   p.ProductCode,
                   p.ProductName,
                   p.Unit,
                   w.PlannedDate,
                   w.TargetQuantity,
                   w.Status,
                   w.StartedAt,
                   w.CompletedAt,
                   w.Memo,
                   w.CreatedAt
            FROM dbo.WorkOrders AS w
            INNER JOIN dbo.Products AS p
                ON p.ProductId = w.ProductId
            ORDER BY w.PlannedDate DESC, w.WorkOrderId DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) {
            workOrders.Add(new WorkOrder {
                WorkOrderId = reader.GetInt32(0),
                WorkOrderNo = reader.GetString(1),
                ProductId = reader.GetInt32(2),
                ProductCode = reader.GetString(3),
                ProductName = reader.GetString(4),
                Unit = reader.GetString(5),

                // SQL의 DATE 값을 날짜 전용 형식으로 변환
                PlannedDate = DateOnly.FromDateTime(reader.GetDateTime(6)),

                TargetQuantity = reader.GetInt32(7),
                Status = reader.GetString(8),

                // NULL일 수 있는 열은 값의 존재 여부부터 확인
                StartedAt = reader.IsDBNull(9)
                    ? null
                    : DateTime.SpecifyKind(
                        reader.GetDateTime(9), DateTimeKind.Utc),

                CompletedAt = reader.IsDBNull(10)
                    ? null
                    : DateTime.SpecifyKind(
                        reader.GetDateTime(10), DateTimeKind.Utc),

                Memo = reader.IsDBNull(11)
                    ? null
                    : reader.GetString(11),

                CreatedAt = DateTime.SpecifyKind(
                    reader.GetDateTime(12), DateTimeKind.Utc)
            });
        }

        return workOrders;
    }
}