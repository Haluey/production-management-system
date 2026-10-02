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

    // 작업지시, 제품 정보, 생산실적 합계와 달성률을 함께 조회
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
               w.CreatedAt,
               COALESCE(r.TotalGoodQuantity, 0) AS TotalGoodQuantity,
               COALESCE(r.TotalDefectQuantity, 0) AS TotalDefectQuantity,
               CAST(
                   COALESCE(r.TotalGoodQuantity, 0) * 100.0
                   / w.TargetQuantity
                   AS DECIMAL(10, 2)
               ) AS AchievementRate
        FROM dbo.WorkOrders AS w
        INNER JOIN dbo.Products AS p
            ON p.ProductId = w.ProductId
        LEFT JOIN (
            SELECT WorkOrderId,
                   SUM(CAST(GoodQuantity AS BIGINT)) AS TotalGoodQuantity,
                   SUM(CAST(DefectQuantity AS BIGINT)) AS TotalDefectQuantity
            FROM dbo.ProductionResults
            GROUP BY WorkOrderId
        ) AS r
            ON r.WorkOrderId = w.WorkOrderId
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

                // 시작·완료 시간은 NULL일 수 있으므로 먼저 확인
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
                    reader.GetDateTime(12), DateTimeKind.Utc),

                // SQL의 BIGINT 합계를 C#의 long으로 읽기
                TotalGoodQuantity = reader.GetInt64(13),
                TotalDefectQuantity = reader.GetInt64(14),

                // SQL의 DECIMAL 달성률을 C#의 decimal로 읽기
                AchievementRate = reader.GetDecimal(15)
            });
        }

        return workOrders;
    }

    // 활성 제품에 새 작업지시 등록
    // 제품이 없거나 비활성이면 null 반환
    public async Task<int?> CreateAsync(
        string workOrderNo,
        int productId,
        DateOnly plannedDate,
        int targetQuantity,
        string? memo) {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
        INSERT INTO dbo.WorkOrders
            (WorkOrderNo, ProductId, PlannedDate, TargetQuantity, Memo)
        OUTPUT INSERTED.WorkOrderId
        SELECT @WorkOrderNo,
               p.ProductId,
               @PlannedDate,
               @TargetQuantity,
               @Memo
        FROM dbo.Products AS p
        WHERE p.ProductId = @ProductId
          AND p.IsActive = 1;
        """;

        command.Parameters.Add(
            "@WorkOrderNo", System.Data.SqlDbType.NVarChar, 30)
            .Value = workOrderNo;

        command.Parameters.Add(
            "@ProductId", System.Data.SqlDbType.Int)
            .Value = productId;

        // 날짜만 저장하도록 SQL 자료형을 Date로 지정
        command.Parameters.Add(
            "@PlannedDate", System.Data.SqlDbType.Date)
            .Value = plannedDate.ToDateTime(TimeOnly.MinValue);

        command.Parameters.Add(
            "@TargetQuantity", System.Data.SqlDbType.Int)
            .Value = targetQuantity;

        // 비고가 없으면 DB의 NULL로 저장
        command.Parameters.Add(
            "@Memo", System.Data.SqlDbType.NVarChar, 500)
            .Value = (object?)memo ?? DBNull.Value;

        var result = await command.ExecuteScalarAsync();

        return result is null || result is DBNull
            ? null
            : Convert.ToInt32(result);
    }

    // 대기 상태의 작업지시만 시작
    public async Task<bool> StartAsync(int workOrderId) {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
        UPDATE dbo.WorkOrders
        SET Status = N'InProgress',
            StartedAt = SYSUTCDATETIME()
        WHERE WorkOrderId = @WorkOrderId
          AND Status = N'Waiting';
        """;

        command.Parameters.Add(
            "@WorkOrderId", System.Data.SqlDbType.Int)
            .Value = workOrderId;

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows == 1;
    }
}