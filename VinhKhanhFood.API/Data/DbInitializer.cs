using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
            await EnsureUsersVipColumnAsync(connection, cancellationToken);
            await EnsureUsersVirtualColumnsAsync(connection, cancellationToken);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private static async Task EnsureUsersVipColumnAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info('Users');";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var columnName = reader.GetString(1);
            if (string.Equals(columnName, "IsVip", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        await reader.CloseAsync();

        await using var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = "ALTER TABLE Users ADD COLUMN IsVip INTEGER NOT NULL DEFAULT 0;";
        await alterCommand.ExecuteNonQueryAsync(cancellationToken);
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
    }

    private static async Task<HashSet<string>> GetUserTableColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info('Users');";

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
}
