import { useEffect, useRef } from 'react'
import { CandlestickSeries, createChart } from 'lightweight-charts'




function OhlcChart({ data, handleVisibleRangeChange }) {
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
        })

        const series = chart.addSeries(CandlestickSeries, {
            upColor: '#2f6fed',
            downColor: '#ed553b',
            borderVisible: false,
        })

        chart.timeScale().subscribeVisibleTimeRangeChange(handleVisibleRangeChange)

        chartRef.current = chart
        seriesRef.current = series

        return () => {
            chart.timeScale().unsubscribeVisibleTimeRangeChange(handleVisibleRangeChange)
            chart.remove()
        }
    }, [handleVisibleRangeChange])

    useEffect(() => {
        if(!seriesRef.current) return;
        if(!Array.isArray(data)) return;
        
        seriesRef.current.setData(data)
    }, [data])

    return <div ref={containerRef} style={{ width: '100%', height: '100%' }} />
}

export default OhlcChart;
