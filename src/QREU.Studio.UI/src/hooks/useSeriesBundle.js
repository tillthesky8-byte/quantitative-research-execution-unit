import { useQuery } from '@tanstack/react-query'
import { fetchBackwardSeriesBundle, fetchForwardSeriesBundle } from '../api/marketApi'
 
export function useBackwardSeriesBundle({ runId, symbol, timeframe, from }) {
    const queryKey = ['series', 'backward-chunk', runId, symbol, timeframe, from]

    const query = useQuery({
        queryKey,
        queryFn: () => fetchBackwardSeriesBundle({ runId, symbol, timeframe, from }),
        enabled: Boolean(runId && symbol && timeframe && from),
        placeholderData: previous => previous,
    })

    console.log('useBackwardSeriesBundle', { queryKey, data: query.data, isLoading: query.isLoading, error: query.error })

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

export function useForwardSeriesBundle({ runId, symbol, timeframe, to }) {
    const queryKey = ['series', 'forward-chunk', runId, symbol, timeframe, to]

    const query = useQuery({
        queryKey,
        queryFn: () => fetchForwardSeriesBundle({ runId, symbol, timeframe, to }),
        enabled: Boolean(runId && symbol && timeframe && to),
        placeholderData: previous => previous,
    })

    console.log('useForwardSeriesBundle', { queryKey, data: query.data, isLoading: query.isLoading, error: query.error })

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