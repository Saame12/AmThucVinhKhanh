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
}
