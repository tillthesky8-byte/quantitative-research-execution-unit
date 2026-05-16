import { Link } from 'react-router-dom'

function RunCard({ run }) {
    return (
        <div style={{ border: '1px solid #ddd', padding: 12 }}>
            <div><strong>Run:</strong> {run.id}</div>
            <div>Strategy: {run.strategyName}</div>
            <div>Dataset: {run.symbols.join(', ')}</div>
            <div>Ran At: {run.ranAt}</div>
            <Link to={`/runs/${run.id}`}>Open</Link>
        </div>
    )
}

export default RunCard
