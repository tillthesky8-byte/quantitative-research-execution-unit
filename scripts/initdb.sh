#!/bin/bash


set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
DATA_DIR="$PROJECT_ROOT/data"
DB_FILE="$DATA_DIR/research.duckdb"
SCHEMA_FILE="$PROJECT_ROOT/sql/init.sql"

if ! command -v duckdb &> /dev/null; then
    echo "Error: duckdb command not found. Please install DuckDB."
    exit 1
fi

if [ ! -f "$SCHEMA_FILE" ]; then
    echo "Error: Schema file not found at $SCHEMA_FILE"
    exit 1
fi

mkdir -p "$DATA_DIR"

if [ -f "$DB_FILE" ]; then
    echo "Removing existing database at $DB_FILE"
    rm "$DB_FILE"
fi

echo "Initializing database at $DB_FILE with schema from $SCHEMA_FILE"
duckdb "$DB_FILE" < "$SCHEMA_FILE"

echo "Database initialized successfully at $DB_FILE"
