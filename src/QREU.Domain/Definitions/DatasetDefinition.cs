using System.Security.Cryptography;
using System.Text;

namespace Domain.Definitions;
public record DatasetDefinition
(
    InstrumentDefinition[] Instruments,
    FactorDefinition[] Factors,
    DateTime StartDate,
    DateTime EndDate
)
{
public string BuildHash()
    {
        var sb = new StringBuilder();


        sb.AppendLine("v1");
        sb.AppendLine($"start:{StartDate.ToUniversalTime():O}");
        sb.AppendLine($"end:{EndDate.ToUniversalTime():O}");

        foreach (var instrument in Instruments)
        {
            sb.AppendLine(instrument.Symbol.ToUpperInvariant());
        }
        if (Factors != null)
        {
            foreach (var factor in Factors)
            {
                sb.AppendLine($"{factor.Symbol.ToUpperInvariant()}:{factor.Name.ToUpperInvariant()}");
            }
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);

    }
}