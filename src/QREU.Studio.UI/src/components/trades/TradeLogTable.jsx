import { useMemo, useState } from 'react'
import { createColumnHelper, getCoreRowModel, getSortedRowModel, useReactTable } from '@tanstack/react-table'
import { useTradeLog } from '../../hooks/useTradeLog'

const columnHelper = createColumnHelper()

const columns = [
    columnHelper.accessor('timestamp', { header: 'Timestamp' }),
    columnHelper.accessor('symbol', { header: 'Symbol' }),
    columnHelper.accessor('side', { header: 'Side' }),
    columnHelper.accessor('qty', { header: 'Qty' }),
    columnHelper.accessor('price', { header: 'Price' }),
    columnHelper.accessor('pnl', { header: 'PnL' }),
    columnHelper.accessor('action', { header: 'Action' }),
]

function TradeLogTable({ runId, from, to }) {
    const [sorting, setSorting] = useState([])
    const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 25 })

    const { rows, total, isLoading, error } = useTradeLog({
        runId,
        from,
        to,
        pageIndex: pagination.pageIndex,
        pageSize: pagination.pageSize,
        sorting,
        filters: [],
    })

    const data = useMemo(() => rows, [rows])

    const table = useReactTable({
        data,
        columns,
        state: { sorting, pagination },
        pageCount: Math.ceil(total / pagination.pageSize),
        manualPagination: true,
        manualSorting: true,
        onSortingChange: setSorting,
        onPaginationChange: setPagination,
        getCoreRowModel: getCoreRowModel(),
        getSortedRowModel: getSortedRowModel(),
    })

    if (isLoading) {
        return <div>Loading trades...</div>
    }

    if (error) {
        return <div>Failed to load trades.</div>
    }

    return (
        <div>
            <table>
                <thead>
                    {table.getHeaderGroups().map(headerGroup => (
                        <tr key={headerGroup.id}>
                            {headerGroup.headers.map(header => (
                                <th key={header.id}>
                                    {header.isPlaceholder
                                        ? null
                                        : header.column.columnDef.header}
                                </th>
                            ))}
                        </tr>
                    ))}
                </thead>
                <tbody>
                    {table.getRowModel().rows.map(row => (
                        <tr key={row.id}>
                            {row.getVisibleCells().map(cell => (
                                <td key={cell.id}>{cell.getValue()}</td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>

            <div style={{ display: 'flex', gap: 8, alignItems: 'center', marginTop: 8 }}>
                <button
                    onClick={() => table.previousPage()}
                    disabled={!table.getCanPreviousPage()}
                >
                    Prev
                </button>
                <span>
                    Page {pagination.pageIndex + 1} / {table.getPageCount()}
                </span>
                <button
                    onClick={() => table.nextPage()}
                    disabled={!table.getCanNextPage()}
                >
                    Next
                </button>
            </div>
        </div>
    )
}

export default TradeLogTable
