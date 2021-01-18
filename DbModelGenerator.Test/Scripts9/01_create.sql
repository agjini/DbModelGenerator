CREATE TABLE contract
(
    id                     SERIAL       NOT NULL,
    code                   VARCHAR(50)  NOT NULL,
    title                  VARCHAR(250) NOT NULL,
    created_by             TEXT,
    creation_date          TIMESTAMP,
    last_modified_by       TEXT,
    last_modification_date TIMESTAMP,
    country_id             INT          NOT NULL,
    UNIQUE (code)
);