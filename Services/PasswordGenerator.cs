using System.Security.Cryptography;
using System.Text;

namespace PasswordManagerCLI.Services;

public class PasswordGenerator
{
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Specials = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    public string Generate(int length = 16, bool useUpper = true, bool useDigits = true, bool useSpecials = true)
    {
        if (length < 4) length = 4; // Lunghezza minima per garantire la presenza di ogni categoria

        var charSet = new StringBuilder(Lowercase);
        var password = new List<char>();

        // Assicuriamo almeno un carattere per tipo tra quelli scelti (Best Practice di Sicurezza)
        if (useUpper)
        {
            charSet.Append(Uppercase);
            password.Add(GetRandomChar(Uppercase));
        }
        if (useDigits)
        {
            charSet.Append(Digits);
            password.Add(GetRandomChar(Digits));
        }
        if (useSpecials)
        {
            charSet.Append(Specials);
            password.Add(GetRandomChar(Specials));
        }

        // Il primo carattere inserito di default è un minuscolo per coprire tutti i set stabili
        password.Add(GetRandomChar(Lowercase));

        // Riempiamo il resto della lunghezza richiesta pescando dal pool globale dei caratteri abilitati
        string finalCharSet = charSet.ToString();
        while (password.Count < length)
        {
            password.Add(GetRandomChar(finalCharSet));
        }

        // Mescoliamo la lista usando il generatore crittografico per non avere i primi caratteri prevedibili
        return ShuffleString(password);
    }

    private char GetRandomChar(string pool)
    {
        // RandomNumberGenerator.GetInt32 garantisce una distribuzione casuale crittograficamente sicura
        int index = RandomNumberGenerator.GetInt32(0, pool.Length);
        return pool[index];
    }

    private string ShuffleString(List<char> chars)
    {
        var shuffled = chars.OrderBy(_ => RandomNumberGenerator.GetInt32(0, 10000)).ToArray();
        return new string(shuffled);
    }
}