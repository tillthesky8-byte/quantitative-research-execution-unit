import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { useSeriesBundle } from '../hooks/useSeriesBundle'
import useChartSync from '../hooks/useChartSync'
import OhlcChart from '../components/charts/OhlcChart'
import EquityChart from '../components/charts/EquityChart'
import { useQuery } from '@tanstack/react-query'
import { fetchRun } from '../api/runApi'
import SymbolSelector from '../components/toolbar/SymbolSelector'

const EMPTY_SYMBOLS = []

function RunViewPage() {
    const { runId } = useParams()

    const { data } = useQuery({
        queryKey: ['runs', runId],
        queryFn: () => fetchRun(runId),
        enabled: Boolean(runId),
    })
    
    const [symbol, setSymbol] = useState('MSFT')
    const symbols = Array.isArray(data?.symbols) ? data.symbols : EMPTY_SYMBOLS
    const selectedSymbol = symbols.includes(symbol) ? symbol : (symbols[0] ?? symbol)
    const { visibleRange, handleVisibleRangeChange } = useChartSync(null);

   

    const { ohlc, equity, isLoading, error } = useSeriesBundle({
        runId,
        symbol: selectedSymbol,
        from: '2019-06-22',
        to: '2019-07-22',
    })


    if (!runId) {
        return <div>Run not found.</div>
    }

    return (
        <div style={{ display: 'grid', gap: 16 }}>
            <SymbolSelector symbols={symbols} value={selectedSymbol} onChange={setSymbol} />
            {error && <div>Failed to load series.</div>}
            {isLoading && <div>Loading series...</div>}

            <div style={{ display: 'grid', gap: 12 }}>
                <div style={{ width: '100%', height: 420 }}>
                    <OhlcChart
                        data={ohlc}
                        handleVisibleRangeChange={handleVisibleRangeChange}
                    />
                </div>
                <div style={{ width: '100%', height: 200 }}>
                    <EquityChart
                        data={equity}
                        visibleRange={visibleRange}
                    />
                </div>
            </div>

            {/* <TradeLogTable runId={runId} from={fromDate} to={toDate} /> */}
        </div>
    )
}

export default RunViewPage
