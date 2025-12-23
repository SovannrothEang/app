namespace CoreAPI.Exceptions;

public sealed class InsufficientBalanceException(decimal amount, decimal requestedAmount) : Exception
{
    public static string Code => "INSUFFICIENT_BALANCE";
    public decimal Amount { get; } = amount;
    public decimal RequestedAmount { get; } = requestedAmount;
}