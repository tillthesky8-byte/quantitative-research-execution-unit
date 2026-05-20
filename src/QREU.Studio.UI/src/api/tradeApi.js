import { apiClient } from './client'

export async function fetchTrades({ runId, pageIndex, pageSize }) {

    return apiClient.getJson('trades', {
        runId,
        page: pageIndex + 1,
        pageSize,
    })
}


export async function fetchTradeMarkers({ runId, pageIndex, pageSize }) {

    return apiClient.getJson('markers', {
        runId,
        page: pageIndex + 1,
        pageSize,

    })
}