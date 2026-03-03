CREATE TABLE IF NOT EXISTS uuid_json_and_text_array
(
    id              UUID NOT NULL,
    json            JSONB NOT NULL,
    text_array      TEXT[] NULL,
    vector          vector(768) NOT NULL,
    PRIMARY KEY (id)
);