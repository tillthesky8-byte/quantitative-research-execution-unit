import { apiClient } from './client'

export async function fetchSeriesBundle({ runId, symbol, timeframe, from, to }) {
    const response = await apiClient.getJson(`series`, {
        runId,
        symbol,
        timeframe,
        from,
        to,
    })
    console.log('Fetched series bundle:', response)
    return response
}
