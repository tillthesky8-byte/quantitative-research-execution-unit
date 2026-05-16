import { useQuery } from '@tanstack/react-query'
import { fetchRuns } from '../api/runApi'
import RunList from '../components/runs/RunList'

function RunsPage() {
    const { data, isLoading, error } = useQuery({
        queryKey: ['runs'],
        queryFn: fetchRuns,
    })

    if (isLoading) {
        return <div>Loading runs...</div>
    }

    if (error) {
        return <div>Failed to load runs.</div>
    }

    return <RunList runs={data || []} />
}

export default RunsPage
