using System.Security.Cryptography;
namespace Domain.Definitions;

public sealed class StrategyDefinition
{
    public required StrategyType Type { get; init; }
    public required Dictionary<string, string> Parameters { get; init; }

    public string BuildHash()
    {
        var input = $"{Type}:{string.Join(";", Parameters.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key.ToUpperInvariant()}={kv.Value.ToUpperInvariant()}"))}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes);
    }
}