# SafeVault CLI 🔐

SafeVault CLI è un **Password Manager** leggero e sicuro da riga di comando sviluppato in **C#** e **.NET 8/9**.
L'applicazione permette di memorizzare, generare e recuperare le proprie credenziali in modo sicuro localmente, isolando i dati sensibili attraverso algoritmi di cifratura avanzati.

---

## 🚀 Funzionalità

- 🛡 **Cifratura Militare:** Utilizza **AES-256-CBC** con derivazione della chiave tramite **PBKDF2** (100.000 iterazioni) per proteggere il database locale.
- 🎨 **Interfaccia CLI Avanzata:** Menu interattivi, tabelle e prompt di input sicuri (mascheramento delle password) grazie a **Spectre.Console**.
- 🎲 **Generatore di Password:** Generatore crittograficamente sicuro (`RandomNumberGenerator`) integrato per creare password robuste e casuali.
- 📋 **Integrazione Clipboard:** Copia immediata della password decifrata negli appunti di sistema senza mostrarla a schermo, prevenendo sguardi indiscreti.
- 💾 **Salvataggio Locale:** Persistenza dei dati in un file JSON locale (`vault.json`).

---

## 📂 Architettura del Progetto

Il progetto segue il principio della **Separazione delle Responsabilità (SRP)**, strutturato nelle seguenti macro-aree:

```text
📁 PasswordManagerCLI
│
├── 📁 Models          # Strutture dati pure (Credential, Vault)
├── 📁 Services        # Logica di business (Cifratura, Storage, Generatore)
├── 📁 UI              # Gestione dell'interfaccia utente (MenuManager)
└── 📄 Program.cs      # Entry point dell'applicazione
```

## 🛠 Requisiti e Dipendenze
Per avviare l'applicazione è necessario il .NET SDK (8.0 o superiore). Il progetto fa uso dei seguenti pacchetti NuGet:

- Spectre.Console - Per la formattazione grafica del terminale.
- TextCopy - Per la gestione multipiattaforma degli appunti di sistema.

## 💻 Come Avviare il Progetto
- Clona la repository:

        git clone [https://github.com/tuo-username/PasswordManagerCLI.git](https://github.com/tuo-username/PasswordManagerCLI.git)

- Entra nella cartella ed esegui il restore dei pacchetti:
 
      dotnet restore

- Avvia l'applicazione:

      dotnet run

## 🔒 Sicurezza
La Master Password inserita all'avvio non viene mai salvata su disco. Viene utilizzata esclusivamente in memoria per derivare la chiave crittografica AES. Se il file vault.json viene rubato, i dati rimangono completamente illeggibili senza la Master Password corretta.

---

## 📈 Roadmap & Migliorie Future
Il progetto è in costante evoluzione. Di seguito sono elencate le funzionalità e le ottimizzazioni pianificate per i prossimi rilasci:

1. Sicurezza Energetica
- [ ] Salt Dinamico Univoco: Generazione di un Salt casuale a 32-bit memorizzato nel JSON per impedire attacchi basati su tabelle precomputate.

- [ ] Auto-Clear Clipboard: Sviluppo di un timer asincrono (Task.Delay) per ripulire gli appunti di sistema dopo 30 secondi dalla copia della password.

- [ ] Verifica Master Password (Sign-up): Implementazione di un flusso di configurazione iniziale per verificare l'integrità della password senza tentare la decifratura diretta.

2. Esperienza Utente (UX)
- [ ] Ricerca Predittiva: Integrazione dei filtri dinamici di Spectre.Console per digitare e cercare i servizi in tempo reale.

- [ ] Categorie e Tag: Supporto per raggruppare le credenziali (es. Lavoro, Personale, Social).

- [ ] Modifica Credenziali: Funzionalità per aggiornare account o password esistenti senza doverli ricreare.

3. Qualità del Codice & Architettura
- [ ] Dependency Injection: Configurazione dell'host generico di .NET per disaccoppiare i servizi e facilitare l'Inversione del Controllo (IoC).

- [ ] Gestione Path Multi-Piattaforma: Spostamento del database vault.json nelle cartelle di sistema standard (es. .config su Linux o AppData su Windows).

- [ ] Unit Testing: Copertura del codice tramite test automatizzati (xUnit) per validare i moduli CryptoService e PasswordGenerator.

4. Implementazione di un database relazionale o non relazionale per il savataggio dei dati