import './SymbolSelector.css'

function SymbolSelector({ symbols, value, onChange }) {
    return (
        <label className="symbol-selector">
            <span className="symbol-selector-label">Symbol</span>
            <select
                className="symbol-selector-input"
                value={value}
                onChange={event => onChange(event.target.value)}
            >
                {symbols.map(symbol => (
                    <option key={symbol} value={symbol}>
                        {symbol}
                    </option>
                ))}
            </select>
        </label>
    )
}

export default SymbolSelector
