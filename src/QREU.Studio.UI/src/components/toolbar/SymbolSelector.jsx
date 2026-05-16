function SymbolSelector({ symbols, value, onChange }) {
    return (
        <label>
            Symbol
            <select
                value={value}
                onChange={event => onChange(event.target.value)}
                style={{ marginLeft: 8 }}
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
