CREATE TABLE tenant
(
    id      SERIAL  NOT NULL,
    name    TEXT,
    groupId INTEGER NOT NULL REFERENCES user_group (id),
    PRIMARY KEY (id)
);

CREATE TABLE user_group
(
    id   SERIAL NOT NULL,
    name TEXT,
    PRIMARY KEY (id)
);