using System.Text.Json;
using Password_Manager.Models;

namespace Password_Manager.Services;

public class StorageService
{
    private readonly string _filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vault.json");
    
    //Funzione che scrive la password nel file JSON
    public void SaveVault(Vault vault)
    {
        var options = new JsonSerializerOptions { WriteIndented = true};
        string json = JsonSerializer.Serialize(vault, options);
        File.WriteAllText(_filepath, json);
    }
    
    //Funzione che carica la password dal file JSON
    public Vault LoadVault()
    {
        if (!File.Exists(_filepath))
        {
            return new Vault();
        }

        string json = File.ReadAllText(_filepath);
        return JsonSerializer.Deserialize<Vault>(json) ?? new Vault();
    }
}