using System;
using Microsoft.Data.SqlClient;

namespace ProductionManagement.Wpf.Data;

public sealed class SqlConnectionFactory {
    private const string ConnectionStringVariable =
        "PRODUCTION_MANAGEMENT_CONNECTION_STRING";

    public SqlConnection CreateConnection() {
        // PC의 환경 변수에서 DB 접속 정보를 읽는다.
        string? connectionString =
            Environment.GetEnvironmentVariable(ConnectionStringVariable);

        // 접속 정보가 없으면 설정이 필요하다는 오류를 전달한다.
        if (string.IsNullOrWhiteSpace(connectionString)) {
            throw new InvalidOperationException(
                $"환경 변수 '{ConnectionStringVariable}'를 설정해 주세요.");
        }

        // 연결 객체를 생성한다. 실제 접속은 OpenAsync() 호출 시 수행한다.
        return new SqlConnection(connectionString);
    }
}