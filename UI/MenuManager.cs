using Spectre.Console;
using Password_Manager.Models;
using Password_Manager.Services;
using TextCopy;

namespace Password_Manager.UI;

public class MenuManager
{
    private readonly CryptoService _cryptoService = new();
    private readonly StorageService _storageService = new();
    private Vault _currentVault = new();
    private string _masterPassword = string.Empty;
    
    //Funzione principale della classe
    public void Run()
    {
        Console.Clear();
        AnsiConsole.Write(new FigletText("SafeVault CLI").Centered().Color(Color.Green));
        
        _masterPassword = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Inserisci la tua Master Password per sbloccare la cassaforte:[/]")
                .Secret());

        _currentVault = _storageService.LoadVault();

        bool running = true;
        while (running)
        {
            Console.WriteLine();
            var scelta = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Seleziona un'operazione:[/]")
                    .AddChoices(new[] { 
                        "📋 Mostra Tutte le Credenziali", 
                        "🔍 Cerca e Copia Password", 
                        "➕ Aggiungi Nuova Password", 
                        "❌ Esci" 
                    }));

            switch (scelta)
            {
                case "📋 Mostra Tutte le Credenziali":
                    MostraTabella();
                    break;
                case "🔍 Cerca e Copia Password":
                    CercaECopiaPassword();
                    break;
                case "➕ Aggiungi Nuova Password":
                    AggiungiNuovaCredenziale();
                    break;
                case "❌ Esci":
                    running = false;
                    AnsiConsole.MarkupLine("[bold red]Cassaforte chiusa. Alla prossima![/]");
                    break;
            }
        }
    }
    
    //Funzione per mostrare la tabella delle credenziali
    private void MostraTabella()
    {
        if (!_currentVault.Credentials.Any())
        {
            AnsiConsole.MarkupLine("[orange]⚠ La cassaforte è vuota.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded).Expand();
        table.AddColumn("[bold green]Servizio / Sito[/]");
        table.AddColumn("[bold blue]Username / Email[/]");
        table.AddColumn("[bold red]Password (Cifrata)[/]");

        foreach (var cred in _currentVault.Credentials)
        {
            table.AddRow(cred.ServiceName, cred.Username, cred.EncryptedPassword);
        }
        
        AnsiConsole.Write(table);
    }
    
    //Funzione per cercare e copiare la password
    private void CercaECopiaPassword()
    {
        if (!_currentVault.Credentials.Any())
        {
            AnsiConsole.MarkupLine("[orange1]⚠ Nessuna credenziale disponibile per la ricerca.[/]");
            return;
        }
        
        var opzioni = _currentVault.Credentials.Select(c => $"{c.ServiceName} ({c.Username})").ToList();
        var selezione = AnsiConsole.Prompt(
            new SelectionPrompt<string>().Title("[yellow]Seleziona l'account di cui vuoi copiare la password:[/]").AddChoices(opzioni));

        var indiceSelezionato = opzioni.IndexOf(selezione);
        var credenzialeScelta = _currentVault.Credentials[indiceSelezionato];

        try
        {
            string passwordDecifrata = _cryptoService.Encrypt(credenzialeScelta.EncryptedPassword, _masterPassword);
            ClipboardService.SetText(passwordDecifrata);
            
            AnsiConsole.MarkupLine($"[bold green]✔ Password per {credenzialeScelta.ServiceName} copiata negli appunti![/]");
        }
        catch (Exception)
        {
            AnsiConsole.MarkupLine("[bold red]❌ Errore durante la decifratura.[/]");
        }
    }
    
    //Funzione per aggiungere nuove credenziali
    private void AggiungiNuovaCredenziale()
    {
        var servizio = AnsiConsole.Ask<string>("[grey]>[/] Nome del Servizio: ").Trim();
        var username = AnsiConsole.Ask<string>("[grey]>[/] Username o Email: ").Trim();
        var passwordInChiaro = AnsiConsole.Prompt(new TextPrompt<string>("[grey]>[/] Inserisci la Password:").Secret());

        string passwordCifrata = _cryptoService.Encrypt(passwordInChiaro, _masterPassword);

        var nuovaCredenziale = new Credential { ServiceName = servizio, Username = username, EncryptedPassword = passwordCifrata};
        _currentVault.Credentials.Add(nuovaCredenziale);
        _storageService.SaveVault(_currentVault);
        
        AnsiConsole.MarkupLine("[bold green]✔ Credenziale salvata con successo![/]");
    }
}