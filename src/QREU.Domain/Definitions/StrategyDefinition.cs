using System.Security.Cryptography;
using System.Text;
namespace Domain.Definitions;

public sealed class StrategyDefinition
{
    public required StrategyType Type { get; init; }
    public required Dictionary<string, string> Parameters { get; init; }

    public string BuildHash()
    {
        var sb = new StringBuilder();
        sb.AppendLine("v1");
        sb.AppendLine($"type:{Type.ToString().ToUpperInvariant()}");
        foreach (var param in Parameters.OrderBy(p => p.Key))
        {
            sb.AppendLine($"{param.Key.ToUpperInvariant()}:{param.Value.ToUpperInvariant()}");
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}