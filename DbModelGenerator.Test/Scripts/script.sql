CREATE TABLE brand
(
    id          SERIAL       NOT NULL,
    name        VARCHAR(50)  NOT NULL,
    logo        VARCHAR(200),
    archived    BOOLEAN DEFAULT '0',
    color       VARCHAR(100) NOT NULL,
    external_id VARCHAR(500),
    PRIMARY KEY (id),
    UNIQUE (name)
);

INSERT INTO country (id, name, code, default_currency_id)
VALUES (1, 'Belgium', 'BE', '1'),
       (2, 'France', '$TEST_VAR$', '1'),
       (4, 'Luxembourg', 'LU', '1'),
       (5, 'Netherlands', 'NL', '1');