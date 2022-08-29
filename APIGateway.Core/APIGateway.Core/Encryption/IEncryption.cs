namespace APIGateway.Core.Encryption
{
    public interface IEncryption
    {
        string Decrypt(string Data);
        string Encrypt(string Data);
    }
}