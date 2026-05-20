import { apiClient } from './client'

export async function fetchBackwardSeriesBundle({ runId, symbol, timeframe, from }) {
    const response = await apiClient.getJson(`series/backward-chunk`, {
        runId,
        symbol,
        timeframe,
        from,
        chunkSize: 10000, 
    })
    console.log('Fetched series bundle:', response)
    return response
}

export async function fetchForwardSeriesBundle({ runId, symbol, timeframe, to }) {
    const response = await apiClient.getJson(`series/forward-chunk`, {
        runId,
        symbol,
        timeframe,
        to,
        chunkSize: 10000, 
    })
    console.log('Fetched series bundle:', response)
    return response
}
