import { useState, useCallback } from "react";

export default function useVisibleTimeRange() {
    const [visibleRange, setVisibleRange] = useState({ from: 0, to: 0 });


    const handleVisibleRangeChange = useCallback(range => {
        console.log('Visible range changed:', range)
        setVisibleRange(range)
    }, [])

    return [visibleRange, handleVisibleRangeChange]
}