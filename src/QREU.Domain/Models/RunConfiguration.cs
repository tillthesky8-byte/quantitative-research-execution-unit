using Domain.Definitions;
using System.Security.Cryptography;
namespace Domain.Models;
public sealed class RunConfiguration
{
    public string?   RunId { get; set; }
    public DateTime? RanAt { get; set; }
    public string? DatasetHash  { get; set; }
    public string? StrategyHash { get; set; }
    public required InstrumentDefinition[] Instruments { get; init; }
    public required FactorDefinition[]     Factors { get; init; }
    public required DateTime               StartDate { get; init; }
    public required DateTime               EndDate { get; init; }
    public required StrategyDefinition     Strategy { get; init; }

    public void Initialize()
    {
        if (string.IsNullOrEmpty(RunId))
            RunId = Guid.NewGuid().ToString();

        if (RanAt == null)
            RanAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(DatasetHash))
            DatasetHash = BuildDatasetHash();

        if (string.IsNullOrEmpty(StrategyHash))
            StrategyHash = Strategy.BuildHash();
    }

    private string BuildDatasetHash()
    {
        var input = $"{string.Join(";", Instruments.OrderBy(i => i.Symbol).Select(i => i.Symbol.ToUpperInvariant()))}:{string.Join(";", Factors.OrderBy(f => f.Symbol).Select(f => f.Symbol.ToUpperInvariant()))}:{StartDate:yyyyMMdd}:{EndDate:yyyyMMdd}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes);
    }
}