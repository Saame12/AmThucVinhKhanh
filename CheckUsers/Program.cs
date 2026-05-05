using Microsoft.Data.Sqlite;

var dbPath = @"C:\Users\sangl\source\repos\AmThucVinhKhanhnew\VinhKhanhFood.API\VinhKhanhFood.db";
var connectionString = $"Data Source={dbPath}";

using var connection = new SqliteConnection(connectionString);
await connection.OpenAsync();

await EnsureColumnAsync(connection, "Subscriptions", "ClaimToken", "TEXT NOT NULL DEFAULT ''");
await EnsureColumnAsync(connection, "Subscriptions", "ClaimedGuestId", "TEXT NULL");
await EnsureColumnAsync(connection, "Subscriptions", "ClaimedAtUtc", "TEXT NULL");

await PrintColumnsAsync(connection, "Subscriptions");
await PrintColumnsAsync(connection, "Users");

static async Task EnsureColumnAsync(SqliteConnection connection, string tableName, string columnName, string columnDefinition)
{
    var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var readCommand = connection.CreateCommand();
    readCommand.CommandText = $"PRAGMA table_info('{tableName}');";

    using (var reader = await readCommand.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            existingColumns.Add(reader.GetString(1));
        }
    }

    if (existingColumns.Contains(columnName))
    {
        return;
    }

    var alterCommand = connection.CreateCommand();
    alterCommand.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};";
    await alterCommand.ExecuteNonQueryAsync();
}

static async Task PrintColumnsAsync(SqliteConnection connection, string tableName)
{
    var command = connection.CreateCommand();
    command.CommandText = $"PRAGMA table_info('{tableName}');";

    using var reader = await command.ExecuteReaderAsync();

    Console.WriteLine($"Columns in {tableName}:");
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"- {reader.GetString(1)} ({reader.GetString(2)})");
    }

    Console.WriteLine();
}
