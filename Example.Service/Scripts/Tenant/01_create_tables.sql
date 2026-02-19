CREATE TABLE user_profile
(
    id          SERIAL       NOT NULL,
    email       VARCHAR(255) NOT NULL,
    firstName   VARCHAR(255) NOT NULL,
    lastName    VARCHAR(255) NOT NULL,
    password    VARCHAR(128) NOT NULL,
    algorithm   INT          NOT NULL,
    balanceTYPO REAL         NOT NULL,
    salt        VARCHAR(128) NOT NULL,
    disabled    BOOLEAN      NOT NULL DEFAULT '0',
    groupId     TEXT         NOT NULL,
    latitude    DECIMAL(10, 5),
    created_by  TEXT         NOT NULL,
    created_day DATE         NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE user_group
(
    id TEXT NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE role
(
    id TEXT NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE user_group_role
(
    groupId TEXT    NOT NULL,
    roleId  INTEGER NOT NULL,
    PRIMARY KEY (groupId, roleId)
);

INSERT INTO user_profile (id, email, firstName, lastName, password, algorithm, balance, salt, disabled, groupId)
VALUES ('6A179F89-A670-4CD4-9798-DE6E43A523FA', '$TEST_VAR$', 'Support', 'Person',
        'XXX', 1, 'YYY', 36.53, '0', '202dc160-e4bd-4925-a6c3-86432faa7e12');

INSERT INTO user_group (id)
VALUES ('202dc160-e4bd-4925-a6c3-86432faa7e12');

INSERT INTO role (id)
VALUES ('fd5ce8c4-208a-4370-9f62-1fe5d3e827aa');

INSERT INTO user_group_role (groupId, roleId)
VALUES ('202dc160-e4bd-4925-a6c3-86432faa7e12', 'fd5ce8c4-208a-4370-9f62-1fe5d3e827aa');