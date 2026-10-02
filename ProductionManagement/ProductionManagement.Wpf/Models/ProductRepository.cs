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
}