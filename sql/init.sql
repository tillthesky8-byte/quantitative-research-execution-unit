CREATE TABLE IF NOT EXISTS runs (
    run_id UUID PRIMARY KEY,
    ran_at BIGINT NOT NULL,
    strategy_hash TEXT NOT NULL,
    dataset_hash TEXT NOT NULL,
    config_json JSON NOT NULL,
    notes TEXT
);

CREATE TABLE IF NOT EXISTS trade_events (
    run_id UUID NOT NULL,
    timestamp BIGINT NOT NULL,
    symbol TEXT NOT NULL,
    action TEXT NOT NULL,
    quantity DOUBLE NOT NULL,
    price DOUBLE NOT NULL,
    commission DOUBLE NOT NULL
);

CREATE TABLE IF NOT EXISTS position_events (
    run_id UUID NOT NULL,
    timestamp BIGINT NOT NULL,
    symbol TEXT NOT NULL,
    quantity DOUBLE NOT NULL,
    entry_price DOUBLE NOT NULL,
    exit_price DOUBLE NOT NULL,
    realized_pnl DOUBLE NOT NULL
);

CREATE TABLE IF NOT EXISTS equity_curve (
    run_id UUID NOT NULL,
    timestamp BIGINT NOT NULL,
    equity DOUBLE NOT NULL,
    cash DOUBLE NOT NULL
);

CREATE TABLE IF NOT EXISTS ohlcv (
    symbol TEXT NOT NULL,
    timestamp BIGINT NOT NULL,

    open DOUBLE NOT NULL,
    high DOUBLE NOT NULL,
    low DOUBLE NOT NULL,
    close DOUBLE NOT NULL,
    volume DOUBLE NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_ohlcv_unique
ON ohlcv(symbol, timestamp);

CREATE INDEX IF NOT EXISTS idx_trade_events_run
ON trade_events(run_id);

CREATE INDEX IF NOT EXISTS idx_position_events_run
ON position_events(run_id);

CREATE INDEX IF NOT EXISTS idx_equity_curve_run
ON equity_curve(run_id);

CREATE INDEX IF NOT EXISTS idx_ohlcv_lookup
ON ohlcv(symbol, timestamp);

CREATE INDEX IF NOT EXISTS idx_trade_events_symbol_time
ON trade_events(symbol, timestamp);

CREATE TABLE IF NOT EXISTS factor_data (
    timestamp BIGINT NOT NULL,
    symbol TEXT NOT NULL,
    name TEXT NOT NULL,
    value DOUBLE NOT NULL
);



CREATE INDEX IF NOT EXISTS idx_factor_data_symbol_name_time
ON factor_data(symbol, name, timestamp);