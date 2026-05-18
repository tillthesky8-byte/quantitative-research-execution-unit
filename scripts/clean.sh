#!/bin/bash

# Clean DuckDB writer tables: removes all records from trade_events, runs_data, equity_curve, realized_pnl_events
# Usage: ./clean.sh [database_path]
# Example: ./clean.sh ./data/data.duckdb

DB_PATH="${1:-./data/data.duckdb}"

if [ ! -f "$DB_PATH" ]; then
    echo "Error: Database file not found at $DB_PATH"
    exit 1
fi

echo "Cleaning tables in: $DB_PATH"
echo "This will remove all records from:"
echo "  - trade_events"
echo "  - runs_data"
echo "  - equity_curve"
echo "  - realized_pnl_events"
echo ""
read -p "Are you sure? (y/n) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Cancelled."
    exit 0
fi

duckdb "$DB_PATH" << EOF
BEGIN TRANSACTION;

-- Delete child tables first (those with foreign keys), then parent
DELETE FROM trade_events;
DELETE FROM equity_curve;
DELETE FROM realized_pnl_events;
DELETE FROM runs_data;

-- Verify all tables are now empty
SELECT COUNT(*) as trade_events_count FROM trade_events;
SELECT COUNT(*) as equity_curve_count FROM equity_curve;
SELECT COUNT(*) as realized_pnl_events_count FROM realized_pnl_events;
SELECT COUNT(*) as runs_data_count FROM runs_data;

COMMIT;
EOF

if [ $? -eq 0 ]; then
    echo "✓ Tables cleaned successfully"
else
    echo "✗ Error: Failed to clean tables. All changes have been rolled back."
    exit 1
fi
