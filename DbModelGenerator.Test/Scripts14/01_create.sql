CREATE TABLE IF NOT EXISTS user_grid_state
(
    id                SERIAL NOT NULL,
    filter_expression TEXT NULL,
    PRIMARY KEY (id)
);