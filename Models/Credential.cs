namespace Password_Manager.Models;

public class Credential
{
    public string ServiceName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
}