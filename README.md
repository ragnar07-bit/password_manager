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