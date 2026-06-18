using Microsoft.Data.SqlClient;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace McpExample.Tools.Data;

[McpServerToolType]
public class SqlServerTool
{
    private readonly string _connectionString;

    // 생성자를 통해 데이터베이스 연결 문자열을 주입받습니다.
    public SqlServerTool()
    {
        // 클래스 내부에서 직접 Configuration을 빌드합니다.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // 실행 폴더에서
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // appsettings.json을 찾습니다.
            .Build();

        // Configuration에서 직접 연결 문자열을 읽어옵니다.
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
    }
    [McpServerTool, Description("Get Table List")]
    public string GetTables()
    {
        var obj = new Dictionary<string, string>()
        {
            {"거래처", "account" }
        };
        return JsonSerializer.Serialize(obj);
    }
    [McpServerTool, Description("Get account table`s Domain")]
    public string GetAccountTableDomain()
    {
        var obj = new Dictionary<string, string>()
        {
            { "account_key", "거래처키, bigint" },
            { "account_code", "거래처코드, nvarchar" },
            { "account_name", "거래처명, nvarchar" },
            { "account_alias", "거래처 별칭, nvarchar" },
            { "is_purchase_office", "매입업체 여부, bit" },
            { "is_sales_office", "매출업체 여부, bit" },
            { "is_use", "사용 여부, bit" },
            { "remarks", "비고, nvarchar" },
            { "log_key", "로그키, bigint" },
            { "account_number", "거래처번호, nvarchar" },
            { "address", "주소, nvarchar" },
            { "ceo_name", "대표자명, nvarchar" },
            { "industry_type", "산업유형, nvarchar" },
            { "industry_class", "산업분류, nvarchar" },
        };
        return JsonSerializer.Serialize(obj);
    }
    [McpServerTool, Description("Executes a read-only SQL SELECT query and returns the result as a JSON string. Only queries starting with 'SELECT' are allowed.")]
    public async Task<string> ExecuteQueryAsync(
        [Description("The SQL query to execute. Must start with 'SELECT'.")] string query)
    {
        // --- 2. SELECT 쿼리 검증 로직 추가 ---
        // 쿼리 앞뒤의 공백을 제거하고, 대소문자 구분 없이 'SELECT'로 시작하는지 확인합니다.
        if (string.IsNullOrWhiteSpace(query) || !query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            // SELECT로 시작하지 않으면 에러 메시지를 JSON 형태로 반환합니다.
            return JsonSerializer.Serialize(new { error = "Invalid query. Only SELECT statements are allowed." });
        }
        // ------------------------------------

        var results = new List<Dictionary<string, object?>>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(query, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            results.Add(row);
        }

        return JsonSerializer.Serialize(results);
    }
}