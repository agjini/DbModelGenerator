CREATE TABLE table1
(
    id   SERIAL    NOT NULL,
    test TIMESTAMP NOT NULL,
    PRIMARY KEY (id)
);

CREATE INDEX test_idx ON table1 (test);

CREATE TABLE table2
(
    id   SERIAL    NOT NULL,
    test TIMESTAMP NOT NULL,
    PRIMARY KEY (id)
);