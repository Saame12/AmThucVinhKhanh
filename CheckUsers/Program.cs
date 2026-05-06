using Microsoft.Data.Sqlite;

var dbPath = @"C:\Users\sangl\source\repos\AmThucVinhKhanhnew\VinhKhanhFood.API\VinhKhanhFood.db";
var connectionString = $"Data Source={dbPath}";

using var connection = new SqliteConnection(connectionString);
await connection.OpenAsync();

await RemoveLegacySubscriptionColumnsAsync(connection);
await PrintColumnsAsync(connection, "Subscriptions");
await PrintColumnsAsync(connection, "Users");

static async Task RemoveLegacySubscriptionColumnsAsync(SqliteConnection connection)
{
    var columns = await GetColumnsAsync(connection, "Subscriptions");
    var legacyColumns = new[] { "ClaimToken", "ClaimedGuestId", "ClaimedAtUtc" };

    if (!legacyColumns.Any(columns.Contains))
    {
        Console.WriteLine("Subscriptions schema is already clean.");
        Console.WriteLine();
        return;
    }

    await using var dbTransaction = await connection.BeginTransactionAsync();
    var transaction = (SqliteTransaction)dbTransaction;

    try
    {
        await ExecuteSqlAsync(connection, transaction, @"
            CREATE TABLE Subscriptions_Clean (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GuestId TEXT NOT NULL,
                PaymentCode TEXT NOT NULL,
                StartDate TEXT NOT NULL,
                EndDate TEXT NOT NULL,
                Status TEXT NOT NULL DEFAULT 'Active',
                Amount REAL NOT NULL,
                CreatedAt TEXT NOT NULL
            );");

        await ExecuteSqlAsync(connection, transaction, @"
            INSERT INTO Subscriptions_Clean (Id, GuestId, PaymentCode, StartDate, EndDate, Status, Amount, CreatedAt)
            SELECT Id, GuestId, PaymentCode, StartDate, EndDate, Status, Amount, CreatedAt
            FROM Subscriptions;");

        await ExecuteSqlAsync(connection, transaction, "DROP TABLE Subscriptions;");
        await ExecuteSqlAsync(connection, transaction, "ALTER TABLE Subscriptions_Clean RENAME TO Subscriptions;");
        await ExecuteSqlAsync(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_subscriptions_guestid ON Subscriptions(GuestId);");
        await ExecuteSqlAsync(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_subscriptions_status ON Subscriptions(Status);");

        await transaction.CommitAsync();

        Console.WriteLine("Removed legacy Claim* columns from Subscriptions.");
        Console.WriteLine();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

static async Task PrintColumnsAsync(SqliteConnection connection, string tableName)
{
    using var reader = await CreateTableInfoCommand(connection, tableName).ExecuteReaderAsync();

    Console.WriteLine($"Columns in {tableName}:");
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"- {reader.GetString(1)} ({reader.GetString(2)})");
    }

    Console.WriteLine();
}

static async Task<HashSet<string>> GetColumnsAsync(SqliteConnection connection, string tableName)
{
    var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    using var reader = await CreateTableInfoCommand(connection, tableName).ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        columns.Add(reader.GetString(1));
    }

    return columns;
}

static SqliteCommand CreateTableInfoCommand(SqliteConnection connection, string tableName)
{
    var command = connection.CreateCommand();
    command.CommandText = $"PRAGMA table_info('{tableName}');";
    return command;
}

static async Task ExecuteSqlAsync(SqliteConnection connection, SqliteTransaction transaction, string sql)
{
    using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = sql;
    await command.ExecuteNonQueryAsync();
}
