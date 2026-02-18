CREATE TABLE IF NOT EXISTS date_time_only
(
    id                SERIAL NOT NULL,
    date              DATE NULL,
    time              TIME NULL,
    PRIMARY KEY (id)
);