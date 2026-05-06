using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.API.Data;

public static class DbInitializer
{
    public static async Task EnsureSchemaAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var connection = (SqliteConnection)context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            await EnsureUsersVirtualColumnsAsync(connection, cancellationToken);
            await EnsureSubscriptionsTableAsync(connection, cancellationToken);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private static async Task EnsureSubscriptionsTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Subscriptions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GuestId TEXT NOT NULL,
                PaymentCode TEXT NOT NULL,
                StartDate TEXT NOT NULL,
                EndDate TEXT NOT NULL,
                Status TEXT NOT NULL DEFAULT 'Active',
                Amount REAL NOT NULL,
                CreatedAt TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_subscriptions_guestid ON Subscriptions(GuestId);
            CREATE INDEX IF NOT EXISTS idx_subscriptions_status ON Subscriptions(Status);
        ";
        await command.ExecuteNonQueryAsync(cancellationToken);

        await RemoveLegacySubscriptionColumnsAsync(connection, cancellationToken);
    }

    private static async Task RemoveLegacySubscriptionColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var columns = await GetTableColumnsAsync(connection, "Subscriptions", cancellationToken);
        var legacyColumns = new[] { "ClaimToken", "ClaimedGuestId", "ClaimedAtUtc" };

        if (!legacyColumns.Any(columns.Contains))
        {
            return;
        }

        await using var dbTransaction = await connection.BeginTransactionAsync(cancellationToken);
        var transaction = (SqliteTransaction)dbTransaction;

        try
        {
            await ExecuteNonQueryAsync(connection, transaction, @"
                CREATE TABLE Subscriptions_Clean (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GuestId TEXT NOT NULL,
                    PaymentCode TEXT NOT NULL,
                    StartDate TEXT NOT NULL,
                    EndDate TEXT NOT NULL,
                    Status TEXT NOT NULL DEFAULT 'Active',
                    Amount REAL NOT NULL,
                    CreatedAt TEXT NOT NULL
                );", cancellationToken);

            await ExecuteNonQueryAsync(connection, transaction, @"
                INSERT INTO Subscriptions_Clean (Id, GuestId, PaymentCode, StartDate, EndDate, Status, Amount, CreatedAt)
                SELECT Id, GuestId, PaymentCode, StartDate, EndDate, Status, Amount, CreatedAt
                FROM Subscriptions;", cancellationToken);

            await ExecuteNonQueryAsync(connection, transaction, "DROP TABLE Subscriptions;", cancellationToken);
            await ExecuteNonQueryAsync(connection, transaction, "ALTER TABLE Subscriptions_Clean RENAME TO Subscriptions;", cancellationToken);
            await ExecuteNonQueryAsync(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_subscriptions_guestid ON Subscriptions(GuestId);", cancellationToken);
            await ExecuteNonQueryAsync(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_subscriptions_status ON Subscriptions(Status);", cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task EnsureUsersVirtualColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var columns = await GetUserTableColumnsAsync(connection, cancellationToken);

        if (!columns.Contains("IsVirtual", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN IsVirtual INTEGER NOT NULL DEFAULT 0;", cancellationToken);
        }

        if (!columns.Contains("GuestId", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN GuestId TEXT NOT NULL DEFAULT '';", cancellationToken);
        }

        if (!columns.Contains("RemoteIp", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN RemoteIp TEXT NULL;", cancellationToken);
        }

        if (!columns.Contains("LastSeenUtc", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN LastSeenUtc TEXT NULL;", cancellationToken);
        }

        if (!columns.Contains("LastLatitude", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN LastLatitude REAL NULL;", cancellationToken);
        }

        if (!columns.Contains("LastLongitude", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN LastLongitude REAL NULL;", cancellationToken);
        }

        if (!columns.Contains("CurrentPoiId", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN CurrentPoiId INTEGER NULL;", cancellationToken);
        }

        if (!columns.Contains("CurrentPoiName", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN CurrentPoiName TEXT NULL;", cancellationToken);
        }

        if (!columns.Contains("SecondaryPoiId", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN SecondaryPoiId INTEGER NULL;", cancellationToken);
        }

        if (!columns.Contains("SecondaryPoiName", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN SecondaryPoiName TEXT NULL;", cancellationToken);
        }

        if (!columns.Contains("LocationZoneStatus", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN LocationZoneStatus TEXT NULL;", cancellationToken);
        }

        if (!columns.Contains("LastAudioHeartbeatUtc", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN LastAudioHeartbeatUtc TEXT NULL;", cancellationToken);
        }

        if (!columns.Contains("CurrentAudioPoiId", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN CurrentAudioPoiId INTEGER NULL;", cancellationToken);
        }

        if (!columns.Contains("CurrentAudioPoiName", StringComparer.OrdinalIgnoreCase))
        {
            await ExecuteAlterAsync(connection, "ALTER TABLE Users ADD COLUMN CurrentAudioPoiName TEXT NULL;", cancellationToken);
        }
    }

    private static Task<HashSet<string>> GetUserTableColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken) =>
        GetTableColumnsAsync(connection, "Users", cancellationToken);

    private static async Task<HashSet<string>> GetTableColumnsAsync(SqliteConnection connection, string tableName, CancellationToken cancellationToken)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName}');";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static async Task ExecuteAlterAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
