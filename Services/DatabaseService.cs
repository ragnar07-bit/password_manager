using Microsoft.Data.Sqlite;
using Password_Manager.Models;

namespace Password_Manager.Services;

public class DatabaseService
{
    // Il database sarà un singolo file locale chiamato "vault.db" salvato nella cartella dell'app
    private readonly string _connectionString = $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vault.db")}";

    public DatabaseService()
    {
        InitializeDatabase();
    }

    // Crea la tabella all'avvio dell'applicazione se non è già presente
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Credentials (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ServiceName TEXT NOT NULL,
                Username TEXT NOT NULL,
                EncryptedPassword TEXT NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    // ➕ CREATE: Salva una nuova credenziale nel database
    public void AddCredential(Credential cred)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Credentials (ServiceName, Username, EncryptedPassword) 
            VALUES ($service, $user, $pass);";
        
        // L'uso dei parametri protegge l'applicazione da attacchi SQL Injection
        command.Parameters.AddWithValue("$service", cred.ServiceName);
        command.Parameters.AddWithValue("$user", cred.Username);
        command.Parameters.AddWithValue("$pass", cred.EncryptedPassword);

        command.ExecuteNonQuery();
    }

    // 📋 READ: Recupera tutte le credenziali memorizzate
    public List<Credential> GetAllCredentials()
    {
        var credentials = new List<Credential>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ServiceName, Username, EncryptedPassword FROM Credentials;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            credentials.Add(new Credential
            {
                Id = reader.GetInt32(0),
                ServiceName = reader.GetString(1),
                Username = reader.GetString(2),
                EncryptedPassword = reader.GetString(3)
            });
        }

        return credentials;
    }

    // ❌ DELETE: Elimina definitivamente una riga tramite il suo ID unico
    public void DeleteCredential(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Credentials WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);

        command.ExecuteNonQuery();
    }
}