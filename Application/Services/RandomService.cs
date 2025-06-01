using Application.Interfaces.Services;

namespace Application.Services
{
    /// <summary>
    /// Service responsible for generating random values.
    /// </summary>
    public class RandomService : IRandomService
    {
        /// <summary>
        /// Generates a random boolean value.
        /// </summary>
        /// <returns>True or false, chosen randomly.</returns>
        public bool GetRandomBoolean()
        {
            return new Random().Next(0, 2) == 0;
        }
    }
}
