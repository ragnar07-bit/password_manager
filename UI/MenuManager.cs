using Spectre.Console;
using Password_Manager.Models;
using Password_Manager.Services;
using PasswordManagerCLI.Services;
using TextCopy;

namespace Password_Manager.UI;

public class MenuManager
{
    private readonly CryptoService _cryptoService = new();
    private readonly StorageService _storageService = new();
    private readonly PasswordGenerator _passwordGenerator = new();
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
                        "❌ Rimuovi Credenziale",
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
                case "❌ Rimuovi Credenziale":
                    RimuoviCredenziale();
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
            // Rimosso qualsiasi riferimento a orange o colori personalizzati. 
            // Qui c'era il bug alla riga 71.
            AnsiConsole.MarkupLine("[yellow]La cassaforte e vuota.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded).Expand();
        table.AddColumn("[bold green]Servizio / Sito[/]");
        table.AddColumn("[bold blue]Username / Email[/]");
        table.AddColumn("[bold red]Password (Cifrata)[/]");

        foreach (var cred in _currentVault.Credentials)
        {
            table.AddRow(cred.ServiceName, cred.Username, "[grey]********[/]");
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
        catch (System.Security.Cryptography.CryptographicException)
        {
            // Questo errore scatta se la chiave derivata dalla Master Password non riesce a decifrare il blocco AES
            AnsiConsole.MarkupLine("[bold red]❌ Errore di decifratura! La Master Password inserita all'avvio non è corretta.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]❌ Errore imprevisto:[/] {ex.Message}");
        }
    }
    
    //Funzione per aggiungere nuove credenziali
    private void AggiungiNuovaCredenziale()
    {
        var servizio = AnsiConsole.Ask<string>("[grey]>[/] Nome del Servizio:").Trim();
        var username = AnsiConsole.Ask<string>("[grey]>[/] Username o Email:").Trim();
        
        // Integrazione con PasswordGenerator
        var generaCasuale = AnsiConsole.Confirm("Vuoi generare automaticamente una password casuale sicura?");
        string passwordInChiaro = null;
    
        if (string.IsNullOrEmpty(servizio) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(passwordInChiaro))
        {
            AnsiConsole.MarkupLine("[red]❌ Campi non validi. Operazione annullata.[/]");
            return;
        }
        
        if (generaCasuale)
        {
            passwordInChiaro = _passwordGenerator.Generate(18);
            AnsiConsole.MarkupLine("[grey]>[/] Password generata automaticamente (Nascosta).");
        }
        else
        {
            passwordInChiaro = AnsiConsole.Prompt(new TextPrompt<string>("[grey]>[/] Inserisci la Password:").Secret());
        }

        string passwordCifrata = _cryptoService.Encrypt(passwordInChiaro, _masterPassword);

        var nuovaCredenziale = new Credential { ServiceName = servizio, Username = username, EncryptedPassword = passwordCifrata };
        _currentVault.Credentials.Add(nuovaCredenziale);
        _storageService.SaveVault(_currentVault);

        AnsiConsole.MarkupLine("[bold green]✔ Credenziale salvata con successo![/]");
    }
    
    private void RimuoviCredenziale()
    {
        if (!_currentVault.Credentials.Any())
        {
            AnsiConsole.MarkupLine("[orange1]⚠ Nessuna credenziale presente da poter rimuovere.[/]");
            return;
        }

        // 1. Permetti all'utente di selezionare quale credenziale eliminare
        var opzioni = _currentVault.Credentials.Select(c => $"{c.ServiceName} ({c.Username})").ToList();
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[red]Seleziona la credenziale da ELIMINARE DEFINITIVAMENTE:[/]")
                .AddChoices(opzioni));

        var indiceSelezionato = opzioni.IndexOf(selection);
        var credenzialeDaRimuovere = _currentVault.Credentials[indiceSelezionato];

        // 2. Chiedi conferma esplicita per sicurezza
        var conferma = AnsiConsole.Confirm($" Sei sicuro di voler eliminare le credenziali per [bold red]{credenzialeDaRimuovere.ServiceName}[/]?");

        if (conferma)
        {
            _currentVault.Credentials.RemoveAt(indiceSelezionato);
            _storageService.SaveVault(_currentVault); // Salva subito le modifiche su file JSON
            AnsiConsole.MarkupLine("[bold green]✔ Credenziale rimossa con successo e cassaforte aggiornata![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow] Annullato. Nessuna modifica effettuata.[/]");
        }
    }
}