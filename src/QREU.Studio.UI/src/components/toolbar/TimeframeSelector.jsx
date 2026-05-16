function TimeframeSelector({ timeframes, value, onChange }) {
    return (
        <label>
            Timeframe
            <select
                value={value}
                onChange={event => onChange(event.target.value)}
                style={{ marginLeft: 8 }}
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
