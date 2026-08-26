namespace StochasticBackend.src.Shared.Services
{
    public interface IHashingService
    {
        string HashValue(string value);

        Task<string> HashValueAsync(string value);

        bool VerifyValue(string value, string hashedValue);

        Task<bool> VerifyValueAsync(string value, string hashedValue);
    }
}
