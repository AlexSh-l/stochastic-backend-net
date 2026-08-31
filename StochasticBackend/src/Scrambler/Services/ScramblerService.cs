using StochasticBackend.src.Scrambler.Configuration;
using StochasticBackend.src.Scrambler.Exceptions;
using StochasticBackend.src.Shared.Services;

namespace StochasticBackend.src.Scrambler.Services
{
    public class ScramblerService(): IScramblerService
    {
        private static readonly SemaphoreSlim _semaphore = new(2, 2);

        private readonly ScramblerFactory _factory = new();

        public async Task PoisonImageAsync(EScramblerTypes scramblerType, string inputPath, string outputPath)
        {
            var scrambler = _factory.GetScrambler(scramblerType);

            var isSemaphoreFree = await _semaphore.WaitAsync(TimeSpan.FromSeconds(15));
            if (!isSemaphoreFree)
            {
                throw new ScramblerException("Image processing engine is currently at max capacity.");
            }

            try
            {
                await scrambler.PoisonImageAsync(inputPath, outputPath);
            }
            finally { _semaphore.Release(); }
        }
    }
}
