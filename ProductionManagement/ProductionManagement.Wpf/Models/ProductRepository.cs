using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;

namespace ProductionManagement.Wpf.Data;

// Products 테이블의 데이터를 조회하는 클래스
public sealed class ProductRepository {
    private readonly SqlConnectionFactory _connectionFactory;

    public ProductRepository(SqlConnectionFactory connectionFactory) {
        _connectionFactory = connectionFactory;
    }

    // 전체 제품을 제품 코드 순서로 조회
    public async Task<List<Product>> GetAllAsync() {
        var products = new List<Product>();

        // DB 연결 생성 및 열기
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // 실행할 조회 SQL 설정
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ProductId,
                   ProductCode,
                   ProductName,
                   Unit,
                   IsActive,
                   CreatedAt
            FROM dbo.Products
            ORDER BY ProductCode;
            """;

        // 조회 결과를 한 행씩 읽기
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) {
            products.Add(new Product {
                ProductId = reader.GetInt32(0),
                ProductCode = reader.GetString(1),
                ProductName = reader.GetString(2),
                Unit = reader.GetString(3),
                IsActive = reader.GetBoolean(4),
                CreatedAt = DateTime.SpecifyKind(
                    reader.GetDateTime(5),
                    DateTimeKind.Utc)
            });
        }

        return products;
    }

    // 새 제품을 등록하고 생성된 ProductId 반환
    public async Task<int> CreateAsync(
        string productCode,
        string productName,
        string unit) {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
        INSERT INTO dbo.Products (ProductCode, ProductName, Unit)
        OUTPUT INSERTED.ProductId
        VALUES (@ProductCode, @ProductName, @Unit);
        """;

        // 입력값을 SQL 문자열에 붙이지 않고 매개변수로 전달
        command.Parameters.Add(
            "@ProductCode", System.Data.SqlDbType.NVarChar, 30)
            .Value = productCode;

        command.Parameters.Add(
            "@ProductName", System.Data.SqlDbType.NVarChar, 100)
            .Value = productName;

        command.Parameters.Add(
            "@Unit", System.Data.SqlDbType.NVarChar, 20)
            .Value = unit;

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    // 선택한 제품의 제품명과 단위 수정
    public async Task<bool> UpdateAsync(
        int productId,
        string productName,
        string unit) {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
        UPDATE dbo.Products
        SET ProductName = @ProductName,
            Unit = @Unit
        WHERE ProductId = @ProductId;
        """;

        command.Parameters.Add(
            "@ProductId", System.Data.SqlDbType.Int)
            .Value = productId;

        command.Parameters.Add(
            "@ProductName", System.Data.SqlDbType.NVarChar, 100)
            .Value = productName;

        command.Parameters.Add(
            "@Unit", System.Data.SqlDbType.NVarChar, 20)
            .Value = unit;

        // 변경된 행이 1개이면 수정 성공
        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows == 1;
    }
}