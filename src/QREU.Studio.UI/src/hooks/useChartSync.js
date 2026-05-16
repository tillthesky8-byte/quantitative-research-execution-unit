import { useState, useCallback } from "react";

export default function useChartSync() {
    const [visibleRange, setVisibleRange] = useState(null)

    const handleVisibleRangeChange = useCallback(range => {
        setVisibleRange(range)
    }, [])

    return { visibleRange, handleVisibleRangeChange }
}