import './TimeframeSelector.css'

function TimeframeSelector({  value, onChange }) {
    const timeframes = ['1m', '5m', '15m', '30m', '1h', '4h', '1d', '1w'];
    return (
        <label className="timeframe-selector">
            <span className="timeframe-selector-label">Timeframe</span>
            <select
                className="timeframe-input"
                value={value}
                onChange={event => onChange(event.target.value)}
            >
                {timeframes.map(timeframe => (
                    <option key={timeframe} value={timeframe}>
                        {timeframe}
                    </option>
                ))}
            </select>
        </label>
    )
}

export default TimeframeSelector
