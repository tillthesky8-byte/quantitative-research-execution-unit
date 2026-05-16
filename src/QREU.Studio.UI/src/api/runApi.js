import { apiClient } from './client'

export async function fetchRuns() {
    return apiClient.getJson('runs')
}

export async function fetchRun(runId) {
    return apiClient.getJson(`runs/${runId}`)
}