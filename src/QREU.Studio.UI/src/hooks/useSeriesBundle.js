import { useQuery } from '@tanstack/react-query'
import { fetchSeriesBundle } from '../api/marketApi'

export function useSeriesBundle({ runId, symbol, from, to }) {
    const queryKey = ['series', runId, symbol, from, to]

    const query = useQuery({
        queryKey,
        queryFn: () => fetchSeriesBundle({ runId, symbol, from, to }),
        enabled: Boolean(runId && symbol && from && to),
        placeholderData: previous => previous,
    })

    console.log('useSeriesBundle', { queryKey, data: query.data, isLoading: query.isLoading, error: query.error })

    const payload = query.data || {}

    console.log('payload', payload)

    return {
        ohlc: payload.ohlc || [],
        equity: payload.equityCurve || [],
        trades: payload.trades || [],
        isLoading: query.isLoading,
        error: query.error,
    }
}
