using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;

namespace ProductionManagement.Wpf.Data;

// 생산실적 데이터 조회
public sealed class ProductionResultRepository {
    private readonly SqlConnectionFactory _connectionFactory;

    public ProductionResultRepository(
        SqlConnectionFactory connectionFactory) {
        _connectionFactory = connectionFactory
            ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    // 지정한 작업지시에 등록된 실적 조회
    public async Task<List<ProductionResult>> GetByWorkOrderAsync(
        int workOrderId) {
        var results = new List<ProductionResult>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT ProductionResultId,
                   WorkOrderId,
                   ProductionDate,
                   GoodQuantity,
                   DefectQuantity,
                   Memo,
                   CreatedAt
            FROM dbo.ProductionResults
            WHERE WorkOrderId = @WorkOrderId
            ORDER BY ProductionDate, ProductionResultId;
            """;

        command.Parameters.Add(
            "@WorkOrderId", System.Data.SqlDbType.Int)
            .Value = workOrderId;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) {
            results.Add(new ProductionResult {
                ProductionResultId = reader.GetInt32(0),
                WorkOrderId = reader.GetInt32(1),
                ProductionDate = DateOnly.FromDateTime(
                    reader.GetDateTime(2)),
                GoodQuantity = reader.GetInt32(3),
                DefectQuantity = reader.GetInt32(4),
                Memo = reader.IsDBNull(5)
                    ? null
                    : reader.GetString(5),
                CreatedAt = DateTime.SpecifyKind(
                    reader.GetDateTime(6),
                    DateTimeKind.Utc)
            });
        }

        return results;
    }

    // 진행 중인 작업지시에 생산실적 등록
    // 작업지시가 없거나 진행 중이 아니면 null 반환
    public async Task<int?> CreateAsync(
        int workOrderId,
        DateOnly productionDate,
        int goodQuantity,
        int defectQuantity,
        string? memo) {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
        INSERT INTO dbo.ProductionResults
            (WorkOrderId, ProductionDate, GoodQuantity, DefectQuantity, Memo)
        OUTPUT INSERTED.ProductionResultId
        SELECT w.WorkOrderId,
               @ProductionDate,
               @GoodQuantity,
               @DefectQuantity,
               @Memo
        FROM dbo.WorkOrders AS w WITH (UPDLOCK, HOLDLOCK)
        WHERE w.WorkOrderId = @WorkOrderId
          AND w.Status = N'InProgress';
        """;

        command.Parameters.Add(
            "@WorkOrderId", System.Data.SqlDbType.Int)
            .Value = workOrderId;

        command.Parameters.Add(
            "@ProductionDate", System.Data.SqlDbType.Date)
            .Value = productionDate.ToDateTime(TimeOnly.MinValue);

        command.Parameters.Add(
            "@GoodQuantity", System.Data.SqlDbType.Int)
            .Value = goodQuantity;

        command.Parameters.Add(
            "@DefectQuantity", System.Data.SqlDbType.Int)
            .Value = defectQuantity;

        command.Parameters.Add(
            "@Memo", System.Data.SqlDbType.NVarChar, 500)
            .Value = (object?)memo ?? DBNull.Value;

        var result = await command.ExecuteScalarAsync();

        return result is null || result is DBNull
            ? null
            : Convert.ToInt32(result);
    }
}