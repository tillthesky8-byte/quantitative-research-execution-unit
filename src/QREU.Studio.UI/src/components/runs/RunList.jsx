import RunCard from './RunCard'

function RunList({ runs }) {
    if (!runs.length) {
        return <div>No runs found.</div>
    }

    return (
        <div style={{ display: 'grid', gap: 12 }}>
            {runs.map(run => (
                <RunCard key={run.id} run={run} />
            ))}
        </div>
    )
}

export default RunList
