namespace StochasticBackend.src.Shared.Services
{
    public class HashingService: IHashingService
    {
        private const int _WORK_FACTOR = 12;

        private static readonly SemaphoreSlim _semaphore = new(2, 2);

        public string HashValue(string value)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(value, _WORK_FACTOR);
        }

        public bool VerifyValue(string value, string hashedValue)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(value, hashedValue);
        }

        public async Task<string> HashValueAsync(string value)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await Task.Run(() => HashValue(value));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> VerifyValueAsync(string value, string hashedValue)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await Task.Run(() => VerifyValue(value, hashedValue));
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
