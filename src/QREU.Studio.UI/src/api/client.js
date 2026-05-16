const API_BASE_URL = 'http://localhost:9999/api/'

async function getJson(path, params = {}) {
    const url = new URL(path, API_BASE_URL)
    Object.entries(params).forEach(([key, value]) => {
        if (value === undefined || value === null || value === '') {
            return
        }
        url.searchParams.set(key, String(value))
    })

    const response = await fetch(url.toString())
    if (!response.ok) {
        const text = await response.text()
        throw new Error(`Request failed: ${response.status} ${text}`)
    }

    return response.json()
}

export const apiClient = {
    getJson,
}
