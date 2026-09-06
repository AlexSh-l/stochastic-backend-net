namespace StochasticBackend.src.Scrambler.DTO
{
    public class ScrambleFileRequestDTO
    {
        public IFormFile? File { get; set; }
        public string FilterName { get; set; } = string.Empty;
    }
}
