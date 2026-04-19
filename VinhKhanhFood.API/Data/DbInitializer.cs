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
            await EnsureUsersVipColumnAsync(connection, cancellationToken);
            await EnsureUsersVirtualColumnsAsync(connection, cancellationToken);
            await EnsurePaymentTransactionsTableAsync(connection, cancellationToken);
            await SeedMockPaymentTransactionsAsync(context, cancellationToken);
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

    private static async Task EnsurePaymentTransactionsTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS PaymentTransactions (
                Id INTEGER NOT NULL CONSTRAINT PK_PaymentTransactions PRIMARY KEY AUTOINCREMENT,
                TransactionCode TEXT NOT NULL,
                PoiId INTEGER NOT NULL,
                PoiName TEXT NOT NULL,
                Amount REAL NOT NULL,
                Currency TEXT NOT NULL,
                PaymentType TEXT NOT NULL,
                Provider TEXT NOT NULL,
                Status TEXT NOT NULL,
                CustomerLabel TEXT NOT NULL,
                Note TEXT NULL,
                CreatedAt TEXT NOT NULL,
                PaidAt TEXT NULL,
                ReconciledAt TEXT NULL
            );
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SeedMockPaymentTransactionsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.PaymentTransactions.AnyAsync(cancellationToken))
        {
            return;
        }

        var pois = await context.FoodLocations
            .OrderBy(item => item.Id)
            .Take(6)
            .ToListAsync(cancellationToken);

        if (pois.Count == 0)
        {
            return;
        }

        var random = new Random(20260419);
        var rows = new List<PaymentTransaction>();
        var statuses = new[] { "Paid", "Paid", "Paid", "Pending", "Failed", "Paid", "Paid" };
        var providers = new[] { "MockQR", "VNPay-Mock", "MoMo-Mock" };

        for (var dayOffset = 0; dayOffset < 45; dayOffset++)
        {
            var createdDate = DateTime.Now.Date.AddDays(-dayOffset);

            foreach (var poi in pois)
            {
                var transactionCount = random.Next(0, 4);
                for (var index = 0; index < transactionCount; index++)
                {
                    var createdAt = createdDate
                        .AddHours(random.Next(8, 23))
                        .AddMinutes(random.Next(0, 60));
                    var status = statuses[random.Next(statuses.Length)];
                    var amount = random.Next(45, 180) * 1000m;
                    DateTime? paidAt = string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase)
                        ? createdAt.AddMinutes(random.Next(1, 6))
                        : null;
                    var reconciledAt = string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase) && random.NextDouble() > 0.35
                        ? paidAt?.AddMinutes(random.Next(10, 90))
                        : null;

                    rows.Add(new PaymentTransaction
                    {
                        TransactionCode = $"QR-{createdAt:yyyyMMdd}-{poi.Id:D3}-{index + 1:D2}-{random.Next(100, 999)}",
                        PoiId = poi.Id,
                        PoiName = poi.Name,
                        Amount = amount,
                        Currency = "VND",
                        PaymentType = "QR_PAYMENT",
                        Provider = providers[random.Next(providers.Length)],
                        Status = status,
                        CustomerLabel = $"guest-{random.Next(1000, 9999)}",
                        Note = string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase) ? "Mock gateway timeout." : null,
                        CreatedAt = createdAt,
                        PaidAt = paidAt,
                        ReconciledAt = reconciledAt
                    });
                }
            }
        }

        if (rows.Count == 0)
        {
            return;
        }

        context.PaymentTransactions.AddRange(rows);
        await context.SaveChangesAsync(cancellationToken);
    }
}
