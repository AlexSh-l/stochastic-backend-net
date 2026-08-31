using StochasticBackend.src.Auth.DTO;
using StochasticBackend.src.Scrambler.Configuration;
using StochasticBackend.src.Shared.Services;

namespace StochasticBackend.src.Scrambler.Services
{
    public interface IScramblerService: ISingletonService
    {
        Task PoisonImageAsync(EScramblerTypes scramblerType, string inputPath, string outputPath);
    }
}
