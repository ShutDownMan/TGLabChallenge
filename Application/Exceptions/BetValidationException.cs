using System;

namespace Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a bet validation fails.
    /// </summary>
    public class BetValidationException : Exception
    {
        public BetValidationException(string message) : base(message) { }
    }
}
