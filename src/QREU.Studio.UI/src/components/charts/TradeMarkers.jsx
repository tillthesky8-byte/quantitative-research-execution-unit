export function buildTradeMarkers(trades = []) {
    return trades.map(trade => {
        const isBuy = String(trade.side || trade.action).toUpperCase() === 'BUY'

        return {
            time: trade.timestamp || trade.time,
            position: isBuy ? 'belowBar' : 'aboveBar',
            color: isBuy ? '#2f9e44' : '#d9480f',
            shape: isBuy ? 'arrowUp' : 'arrowDown',
            text: `${trade.action || trade.side || ''} ${trade.qty || trade.quantity || ''}`.trim(),
        }
    })
}
