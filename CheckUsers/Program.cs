using Microsoft.Data.Sqlite;

var dbPath = @"C:\Users\sangl\source\repos\AmThucVinhKhanhnew\VinhKhanhFood.API\VinhKhanhFood.db";
var connectionString = $"Data Source={dbPath}";

using var connection = new SqliteConnection(connectionString);
await connection.OpenAsync();

var command = connection.CreateCommand();
command.CommandText = @"
    SELECT Id, Username, Password, FullName, Role, Status
    FROM Users
    ORDER BY Id;
";

using var reader = await command.ExecuteReaderAsync();

Console.WriteLine("All Users in Database:");
Console.WriteLine("=====================================");
Console.WriteLine($"{"Id",-5} {"Username",-20} {"Password",-15} {"FullName",-25} {"Role",-15} {"Status",-10}");
Console.WriteLine(new string('-', 95));

while (await reader.ReadAsync())
{
    var id = reader.GetInt32(0);
    var username = reader.IsDBNull(1) ? "" : reader.GetString(1);
    var password = reader.IsDBNull(2) ? "" : reader.GetString(2);
    var fullName = reader.IsDBNull(3) ? "" : reader.GetString(3);
    var role = reader.IsDBNull(4) ? "" : reader.GetString(4);
    var status = reader.IsDBNull(5) ? "" : reader.GetString(5);

    Console.WriteLine($"{id,-5} {username,-20} {password,-15} {fullName,-25} {role,-15} {status,-10}");
}
