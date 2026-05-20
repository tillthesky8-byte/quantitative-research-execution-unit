import { useQuery } from '@tanstack/react-query'
import { fetchTradeMarkers, fetchTrades } from '../api/tradeApi'

export function useTrades({ runId, pageIndex, pageSize }) {
    const queryKey = ['trades', runId, pageIndex, pageSize]

    const query = useQuery({
        queryKey,
        queryFn: () => fetchTrades({ runId, pageIndex, pageSize }),
        enabled: Boolean(runId),
        placeholderData: previous => previous,
    })

    const payload = query.data || []
    const rows = Array.isArray(payload) ? payload : payload.items || payload.trades || []
    const total = Array.isArray(payload) ? rows.length : payload.total || rows.length

    return {
        rows,
        total,
        isLoading: query.isLoading,
        error: query.error,
    }
}


export function useTradeMarkers({ runId, pageIndex, pageSize }) {
    const queryKey = ['tradeMarkers', runId, pageIndex, pageSize]

    const query = useQuery({
        queryKey,
        queryFn: () => fetchTradeMarkers({ runId, pageIndex, pageSize }),
        enabled: Boolean(runId),
        placeholderData: previous => previous,
    })

    const payload = query.data || []
    console.log('Fetched trades for markers:', payload)

    return { tradeMarkers: payload, isLoading: query.isLoading, error: query.error }
}