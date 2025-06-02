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

    /// <summary>
    /// Exception thrown when a wallet is not found.
    /// </summary>
    public class WalletNotFoundException : Exception
    {
        public WalletNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when a game is not found.
    /// </summary>
    public class GameNotFoundException : Exception
    {
        public GameNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when a player is not found.
    /// </summary>
    public class PlayerNotFoundException : Exception
    {
        public PlayerNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when a wallet has insufficient balance.
    /// </summary>
    public class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when a bet currency does not match the game's requirements.
    /// </summary>
    public class InvalidCurrencyException : Exception
    {
        public InvalidCurrencyException(string message) : base(message) { }
    }
}
