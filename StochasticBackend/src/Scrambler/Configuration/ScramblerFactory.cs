using System.Reflection;

namespace StochasticBackend.src.Scrambler.Configuration
{
    public class ScramblerFactory
    {
        private static readonly Dictionary<EScramblerTypes, Type> _scramblerFilters = new();

        public ScramblerFactory()
        {
            var concreteTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IScrambler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (EScramblerTypes typeEnum in Enum.GetValues<EScramblerTypes>())
            {
                string expectedClassName = $"{typeEnum}";

                var matchingType = concreteTypes.FirstOrDefault(t =>
                    string.Equals(t.Name, expectedClassName, StringComparison.OrdinalIgnoreCase));

                if (matchingType != null)
                {
                    _scramblerFilters[typeEnum] = matchingType;
                }
            }
        }

        public IScrambler GetScrambler(EScramblerTypes type)
        {
            if (!_scramblerFilters.TryGetValue(type, out var targetType))
            {
                throw new ArgumentException($"No class found for EScramblerTypes value: {type}");
            }

            return (IScrambler)Activator.CreateInstance(targetType)!;
        }
    }
}
