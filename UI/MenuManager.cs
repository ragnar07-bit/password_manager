using Spectre.Console;
using Password_Manager.Models;
using Password_Manager.Services;
using PasswordManagerCLI.Services;
using TextCopy;

namespace Password_Manager.UI;

public class MenuManager
{
    private readonly CryptoService _cryptoService = new();
    private readonly PasswordGenerator _passwordGenerator = new();
    private readonly DatabaseService _databaseService = new();
    private string _masterPassword = string.Empty;
    
    //Funzione principale della classe
    public void Run()
    {
        Console.Clear();
        AnsiConsole.Write(new FigletText("SafeVault CLI").Centered().Color(Color.Green));
        
        _masterPassword = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Inserisci la tua Master Password per sbloccare la cassaforte:[/]")
                .Secret());

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
        var credentials = _databaseService.GetAllCredentials();

        if (!credentials.Any())
        {
            AnsiConsole.MarkupLine("[yellow]La cassaforte è vuota.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded).Expand();
        table.AddColumn("[bold green]Servizio / Sito[/]");
        table.AddColumn("[bold blue]Username / Email[/]");
        table.AddColumn("[bold red]Password (Cifrata)[/]");

        foreach (var cred in credentials)
        {
            table.AddRow(cred.ServiceName, cred.Username, "[grey]********[/]");
        }

        AnsiConsole.Write(table);
    }

    private void CercaECopiaPassword()
    {
        var credentials = _databaseService.GetAllCredentials();

        if (!credentials.Any())
        {
            AnsiConsole.MarkupLine("[yellow]⚠ Nessuna credenziale disponibile per la ricerca.[/]");
            return;
        }

        var opzioni = credentials.Select(c => $"{c.ServiceName} ({c.Username})").ToList();
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>().Title("[yellow]Seleziona l'account di cui vuoi copiare la password:[/]").AddChoices(opzioni));

        var indiceSelezionato = opzioni.IndexOf(selection);
        var credenzialeScelta = credentials[indiceSelezionato];

        try
        {
            string passwordDecifrata = _cryptoService.Encrypt(credenzialeScelta.EncryptedPassword, _masterPassword);
            ClipboardService.SetText(passwordDecifrata);
            
            AnsiConsole.MarkupLine($"[bold green]✔ Password per {credenzialeScelta.ServiceName} copiata negli appunti![/]");
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            AnsiConsole.MarkupLine("[bold red]❌ Errore di decifratura! La Master Password inserita all'avvio non è corretta.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]❌ Errore imprevisto:[/] {ex.Message}");
        }
    }

    private void AggiungiNuovaCredenziale()
    {
        var servizio = AnsiConsole.Ask<string>("[grey]>[/] Nome del Servizio:").Trim();
        var username = AnsiConsole.Ask<string>("[grey]>[/] Username o Email:").Trim();
        
        var generaCasuale = AnsiConsole.Confirm("Vuoi generare automaticamente una password casuale sicura?");
        string passwordInChiaro;

        if (generaCasuale)
        {
            passwordInChiaro = _passwordGenerator.Generate(18);
            AnsiConsole.MarkupLine("[grey]>[/] Password generata automaticamente (Nascosta).");
        }
        else
        {
            passwordInChiaro = AnsiConsole.Prompt(new TextPrompt<string>("[grey]>[/] Inserisci la Password:").Secret());
        }

        if (string.IsNullOrEmpty(servizio) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(passwordInChiaro))
        {
            AnsiConsole.MarkupLine("[red]❌ Campi non validi. Operazione annullata.[/]");
            return;
        }

        string passwordCifrata = _cryptoService.Encrypt(passwordInChiaro, _masterPassword);

        // Creiamo il modello e lo passiamo al database
        var nuovaCredenziale = new Credential { ServiceName = servizio, Username = username, EncryptedPassword = passwordCifrata };
        _databaseService.AddCredential(nuovaCredenziale);

        AnsiConsole.MarkupLine("[bold green]✔ Credenziale salvata nel database SQLite con successo![/]");
    }

    private void RimuoviCredenziale()
    {
        var credentials = _databaseService.GetAllCredentials();

        if (!credentials.Any())
        {
            AnsiConsole.MarkupLine("[yellow]⚠ Nessuna credenziale presente da poter rimuovere.[/]");
            return;
        }

        var opzioni = credentials.Select(c => $"{c.ServiceName} ({c.Username})").ToList();
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[red]Seleziona la credenziale da ELIMINARE DEFINITIVAMENTE:[/]")
                .AddChoices(opzioni));

        var indiceSelezionato = opzioni.IndexOf(selection);
        var credenzialeDaRimuovere = credentials[indiceSelezionato];

        var conferma = AnsiConsole.Confirm($"Sei sicuro di voler eliminare le credenziali per [bold red]{credenzialeDaRimuovere.ServiceName}[/]?");

        if (conferma)
        {
            // Questa è la svolta: eliminiamo puntando all'ID univoco del record sul DB!
            _databaseService.DeleteCredential(credenzialeDaRimuovere.Id);
            AnsiConsole.MarkupLine("[bold green]✔ Credenziale rimossa dal database con successo![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Annullato. Nessuna modifica effettuata.[/]");
        }
    }
}