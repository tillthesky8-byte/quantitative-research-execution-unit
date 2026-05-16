import { apiClient } from './client'

export async function fetchTrades({ runId, from, to, pageIndex, pageSize, sorting, filters }) {
    const sortParam = sorting?.length
        ? `${sorting[0].id}:${sorting[0].desc ? 'desc' : 'asc'}`
        : undefined

    const filtersParam = filters?.length ? JSON.stringify(filters) : undefined

    return apiClient.getJson('trades', {
        runId,
        from,
        to,
        page: pageIndex + 1,
        pageSize,
        sort: sortParam,
        filters: filtersParam,
    })
}
