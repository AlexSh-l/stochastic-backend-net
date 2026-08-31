namespace StochasticBackend.src.Scrambler.Configuration
{
    public interface IScrambler
    {
        Task PoisonImageAsync(string inputPath, string outputPath);
        void PoisonImage(string inputPath, string outputPath);
    }
}
