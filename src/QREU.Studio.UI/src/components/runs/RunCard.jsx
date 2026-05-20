import { Link } from 'react-router-dom'
import './RunCard.css'

function RunCard({ run }) {
    const status = run.status || 'completed'
    const symbols = Array.isArray(run.symbols) ? run.symbols.join(', ') : 'N/A'
    const ranAt = run.ranAt ? new Date(run.ranAt).toLocaleString() : 'Unknown'

    return (
        <div className="run-card">
            <div className="run-card-header">
                <h3 className="run-card-title">Run {new Date(run.startDate).toDateString()} - {new Date(run.endDate).toDateString()}</h3>
                <span className={`run-card-status ${status}`}>{status}</span>
            </div>
            <div className="run-card-meta">
                <div className="run-card-meta-item">
                    <span className="run-card-meta-label">Strategy</span>
                    <span className="run-card-meta-value">{run.strategyName || 'N/A'}</span>
                </div>
                <div className="run-card-meta-item">
                    <span className="run-card-meta-label">Symbols</span>
                    <span className="run-card-meta-value">{symbols}</span>
                </div>
                <div className="run-card-meta-item">
                    <span className="run-card-meta-label">Ran At</span>
                    <span className="run-card-meta-value">{ranAt}</span>
                </div>
            </div>
            <Link className="run-card-link" to={`/runs/${run.id}`}>Open dashboard</Link>
        </div>
    )
}

export default RunCard
