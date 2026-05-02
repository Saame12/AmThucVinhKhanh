using System;
using System.Data.SQLite;

class Program {
    static void Main() {
        using var conn = new SQLiteConnection("Data Source=VinhKhanhFood.API/VinhKhanhFood.db");
        conn.Open();
        
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, Password, Role, Status FROM Users WHERE Role != 'TravelerGuest' LIMIT 10";
        
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("Users in database:");
        while (reader.Read()) {
            Console.WriteLine($"Id: {reader["Id"]}, Username: {reader["Username"]}, Password: {reader["Password"]}, Role: {reader["Role"]}, Status: {reader["Status"]}");
        }
    }
}
