CREATE TABLE group_type
(
    id   SERIAL      NOT NULL,
    code VARCHAR(20) NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE group_type_available_scope
(

    group_type_id            INT          NOT NULL REFERENCES group_type (id),
    model_id                 VARCHAR(100) NOT NULL,
    allow_all                BOOLEAN      NOT NULL,
    allow_selected           BOOLEAN      NOT NULL,
    allow_selected_brands    BOOLEAN      NOT NULL,
    allow_selected_countries BOOLEAN      NOT NULL,
    allow_selected_manager   BOOLEAN      NOT NULL,
    allow_filter             BOOLEAN      NOT NULL,

    PRIMARY KEY (group_type_id, model_id)
);

CREATE TABLE role
(
    id        VARCHAR(36)  NOT NULL,
    role_name VARCHAR(256) NOT NULL,
    is_system BOOLEAN      NOT NULL DEFAULT '0',
    PRIMARY KEY (id)
);

CREATE TABLE role_rights
(
    role_id     VARCHAR(36)  NOT NULL REFERENCES role (id),
    section2_id VARCHAR(100) NOT NULL,
    action      VARCHAR(50)  NOT NULL,
    PRIMARY KEY (role_id, section2_id, action)
);


CREATE TABLE "user_group"
(
    id            SERIAL      NOT NULL,
    disabled      BOOLEAN     NOT NULL DEFAULT '0',
    group_type_id INT         NOT NULL,
    modified_on   TIMESTAMP   NULL,
    modified_by   VARCHAR(36) NULL,
    PRIMARY KEY (id)
);

CREATE TABLE user_profile
(
    id                            VARCHAR(36)  NOT NULL,
    email                         VARCHAR(256) NOT NULL,
    first_name                    VARCHAR(100) NOT NULL,
    last_name                     VARCHAR(100) NOT NULL,
    title                         VARCHAR(100) NULL,
    organization                  VARCHAR(100) NULL,
    phone                         VARCHAR(50)  NULL,
    mobile                        VARCHAR(50)  NULL,
    culture_id                    VARCHAR(5)   NOT NULL DEFAULT 'fr-FR',
    home_page                     VARCHAR(500) NULL,
    api_key                       VARCHAR(50)  NULL,
    status                        VARCHAR(50)  NOT NULL,
    password                      VARCHAR(128) NOT NULL,
    salt                          VARCHAR(128) NOT NULL,
    algorithm                     INT          NOT NULL,
    valid_until                   DATE         NULL,
    user_group_id                 INT          NOT NULL REFERENCES "user_group" (id),
    reset_password_token          VARCHAR(50)  NULL,
    invitation_date               DATE         NULL,
    invitation_password_token     VARCHAR(50)  NULL,
    allow_ticketing               BOOLEAN      NOT NULL DEFAULT '0',
    is_admin                      BOOLEAN      NOT NULL DEFAULT '0',
    last_login_date               TIMESTAMP    NULL,
    last_password_changed_date    TIMESTAMP    NULL,
    failed_password_attempt_count INT          NULL,
    failed_password_attempt_start TIMESTAMP    NULL,
    is_locked_out                 BOOLEAN      NOT NULL DEFAULT '0',
    create_date                   TIMESTAMP    NULL,
    modified_on                   TIMESTAMP    NULL,
    modified_by                   VARCHAR(36)  NULL,
    PRIMARY KEY (id),
    UNIQUE (email)
);

CREATE TABLE user_group_role
(
    user_group_id INT         NOT NULL REFERENCES user_group (id),
    role_id       VARCHAR(36) NOT NULL REFERENCES role (id),
    PRIMARY KEY (user_group_id, role_id)
);

CREATE TABLE user_scope_access
(
    user_id           VARCHAR(36)  NOT NULL REFERENCES user_profile (id),
    model_id          VARCHAR(100) NOT NULL,
    scope_type        VARCHAR(50)  NOT NULL,
    selected_id       TEXT         NULL,
    filter_expression TEXT         NULL,
    filter_value      TEXT         NULL,
    PRIMARY KEY (user_id, model_id)
);

CREATE TABLE user_grid_state
(
    id                     SERIAL       NOT NULL,
    user_profile_id        VARCHAR(36)  NOT NULL,
    section2_id            VARCHAR(100) NOT NULL,
    name                   VARCHAR(255) NOT NULL,
    json_state             TEXT         NULL,
    creation_date          TIMESTAMP    NULL,
    last_modification_date TIMESTAMP    NULL,
    filter_expression      TEXT         NULL,
    PRIMARY KEY (id)
);
