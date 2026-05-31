using System.Text;

namespace Password_Manager.Services;

public class CryptoService
{
    //Funzione che cripta la password
    public string Encrypt(string plainText, string masterPassword)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }
}