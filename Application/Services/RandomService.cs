using Application.Interfaces.Services;

namespace Application.Services
{
    public class RandomService : IRandomService
    {
        public bool GetRandomBoolean()
        {
            return new Random().Next(0, 2) == 0;
        }
    }
}
