using Domain.Models;

namespace Domain.Interfaces;

public interface IIndicator
{
    void Update(IReadOnlyDictionary<string,  SymbolState> symbolStates);
    bool IsReady { get; }
}