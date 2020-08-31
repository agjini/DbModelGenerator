CREATE TABLE tenant
(
    id      SERIAL  NOT NULL,
    name    TEXT DEFAULT('biblic'),
    groupId INTEGER NOT NULL REFERENCES user_group (id),
    PRIMARY KEY (id)
);

CREATE TABLE user_group
(
    id   INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    name TEXT
);