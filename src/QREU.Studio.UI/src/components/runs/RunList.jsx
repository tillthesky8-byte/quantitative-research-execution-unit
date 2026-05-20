import RunCard from './RunCard'
import './RunList.css'

function RunList({ runs }) {
    if (!runs.length) {
        return (
            <div className="run-list-empty">
                <div className="run-list-empty-icon">[+]</div>
                <div className="run-list-empty-text">No runs found yet.</div>
                <div className="run-list-empty-subtext">Kick off a new run to start tracking results.</div>
            </div>
        )
    }

    return (
        <div className="run-list">
            {runs.map(run => (
                <RunCard key={run.id} run={run} />
            ))}
        </div>
    )
}

export default RunList
