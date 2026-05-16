import { useQuery } from '@tanstack/react-query'
import { fetchTrades } from '../api/tradeApi'

export function useTradeLog({ runId, from, to, pageIndex, pageSize, sorting, filters }) {
    const queryKey = ['trades', runId, from, to, pageIndex, pageSize, sorting, filters]

    const query = useQuery({
        queryKey,
        queryFn: () => fetchTrades({ runId, from, to, pageIndex, pageSize, sorting, filters }),
        enabled: Boolean(runId && from && to),
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
