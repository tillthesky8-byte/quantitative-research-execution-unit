const API_BASE_URL = 'http://localhost:9999/api/'

async function getJson(path, params = {}) {
    const url = new URL(path, API_BASE_URL)
    Object.entries(params).forEach(([key, value]) => {
        if (value === undefined || value === null || value === '') {
            return
        }
        url.searchParams.set(key, String(value))
    })

    console.log('[API] Fetching:', url.toString())
    try {
        const response = await fetch(url.toString())
        console.log('[API] Response status:', response.status)
        if (!response.ok) {
            const text = await response.text()
            console.error('[API] Error response:', text)
            throw new Error(`Request failed: ${response.status} ${text}`)
        }
        const data = await response.json()
        console.log('[API] Success:', data)
        return data
    } catch (error) {
        console.error('[API] Fetch error:', error)
        throw error
    }
}

export const apiClient = {
    getJson,
}
