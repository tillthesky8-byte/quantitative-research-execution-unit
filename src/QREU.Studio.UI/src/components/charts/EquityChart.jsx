import { useEffect, useRef } from 'react'
import { LineSeries, createChart } from 'lightweight-charts'


function EquityChart({ data, visibleRange }) {
    const containerRef = useRef(null)
    const chartRef = useRef(null)
    const seriesRef = useRef(null)

    useEffect(() => {
        if (!containerRef.current) return;

        const chart = createChart(containerRef.current, {
            width: containerRef.current.clientWidth,
            height: containerRef.current.clientHeight,
            layout: {
                backgroundColor: '#ffffff',
                textColor: '#333',
            },
            grid: {
                vertLines: {
                    color: '#eee',
                },
                horzLines: {
                    color: '#eee',
                },
            },
        } )

        const series = chart.addSeries(LineSeries,{
            color: '#2f6fed',
            lineWidth: 2,
        })

        chartRef.current = chart
        seriesRef.current = series

        return () => {
            chart.remove()
        }
    }, [])

    useEffect(() => {
        if(!seriesRef.current) return;
        if(!Array.isArray(data)) return;
        
        seriesRef.current.setData(data)
    }, [data])

    useEffect(() => {
        if (!chartRef.current) return;
        if (!visibleRange) return;


        const hasValidBounds =
            visibleRange &&
            typeof visibleRange.from === 'number' &&
            typeof visibleRange.to === 'number'

        if (!hasValidBounds) return

        const current = chartRef.current.timeScale().getVisibleLogicalRange()
        if (!current) return

        chartRef.current.timeScale().setVisibleRange(visibleRange)
    }, [visibleRange])

    return <div ref={containerRef} style={{ width: '100%', height: '100%' }} />
}

export default EquityChart
