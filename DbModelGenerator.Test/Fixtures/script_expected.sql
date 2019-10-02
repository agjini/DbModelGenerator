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
CREATE TABLE referential_type
(
    id                      SERIAL      NOT NULL,
    code                    VARCHAR(50) NOT NULL,
    active                  BOOLEAN     NOT NULL,
    icon                    VARCHAR(1000),
    position                INTEGER     NOT NULL,
    referential_entity_type SMALLINT    NOT NULL,
    PRIMARY KEY (id),
    UNIQUE (code)
);
CREATE TABLE datagroup
(
    id                  SERIAL              NOT NULL,
    is_default          BOOLEAN DEFAULT '0' NOT NULL,
    entity_type         VARCHAR(50)         NOT NULL,
    position            INTEGER DEFAULT 0   NOT NULL,
    referential_type_id INTEGER             NOT NULL REFERENCES referential_type (id),
    PRIMARY KEY (id)
);
CREATE TABLE configuration_field
(
    id                        SERIAL              NOT NULL,
    code                      VARCHAR(50)         NOT NULL,
    data_group_id             INTEGER             NOT NULL REFERENCES datagroup (id),
    is_system_type            BOOLEAN DEFAULT '0' NOT NULL,
    is_visible                BOOLEAN             NOT NULL,
    is_mandatory              BOOLEAN             NOT NULL,
    is_locked                 BOOLEAN             NOT NULL,
    is_visible_only_view_mode BOOLEAN DEFAULT '0' NOT NULL,
    is_keep_history           BOOLEAN DEFAULT '0' NOT NULL,
    value_type                VARCHAR(100)        NOT NULL,
    position                  INTEGER             NOT NULL,
    parent_id                 INTEGER REFERENCES configuration_field (id),
    no_parameter_list         BOOLEAN DEFAULT '0' NOT NULL,
    is_personal_data          BOOLEAN             NOT NULL,
    is_visible_map            BOOLEAN DEFAULT '0' NOT NULL,
    is_criteria               BOOLEAN DEFAULT '0' NOT NULL,
    PRIMARY KEY (id),
    UNIQUE (code, data_group_id)
);
CREATE TABLE configuration_field_list_item
(
    id                     SERIAL  NOT NULL,
    code                   VARCHAR(50),
    configuration_field_id INTEGER NOT NULL REFERENCES configuration_field (id),
    position               INTEGER NOT NULL,
    PRIMARY KEY (id)
);
CREATE TABLE currency
(
    id     SERIAL       NOT NULL,
    code   VARCHAR(3)   NOT NULL,
    symbol VARCHAR(10),
    name   VARCHAR(100) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE (code)
);
CREATE TABLE country
(
    id                  SERIAL       NOT NULL,
    code                VARCHAR(2)   NOT NULL,
    name                VARCHAR(100) NOT NULL,
    default_currency_id INTEGER      NOT NULL REFERENCES currency (id),
    PRIMARY KEY (id),
    UNIQUE (code)
);
CREATE TABLE manager
(
    id                SERIAL              NOT NULL,
    user_profile_id   TEXT                NOT NULL,
    invitation_status VARCHAR(50)         NOT NULL,
    function          VARCHAR(50)         NOT NULL,
    firstname         VARCHAR(50)         NOT NULL,
    lastname          VARCHAR(50)         NOT NULL,
    archived          BOOLEAN DEFAULT '0' NOT NULL,
    PRIMARY KEY (id)
);
CREATE TABLE nace
(
    id    SERIAL       NOT NULL,
    code  VARCHAR(50)  NOT NULL,
    label VARCHAR(150) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE (code)
);
CREATE TABLE location_type
(
    id          serial not null,
    external_id varchar(500),
    PRIMARY KEY (id)
);
CREATE TABLE operating_mode
(
    id        SERIAL NOT NULL,
    shape_url VARCHAR(2083),
    svg_path  VARCHAR(2083),
    code      VARCHAR(50),
    PRIMARY KEY (id),
    UNIQUE (code)
);
CREATE TABLE concept
(
    id   SERIAL NOT NULL,
    code VARCHAR(50),
    PRIMARY KEY (id),
    UNIQUE (code)
);
CREATE TABLE states
(
    id         SERIAL  NOT NULL,
    country_id INTEGER NOT NULL REFERENCES country (id),
    name       VARCHAR(250),
    PRIMARY KEY (id),
    UNIQUE (name)
);
CREATE TABLE zone
(
    id              SERIAL NOT NULL,
    code            VARCHAR(50),
    zone_manager_id INTEGER REFERENCES manager (id),
    PRIMARY KEY (id),
    UNIQUE (code)
);
CREATE TABLE region
(
    id                  SERIAL NOT NULL,
    regional_manager_id INTEGER REFERENCES manager (id),
    brand_id            INTEGER REFERENCES brand (id),
    code                VARCHAR(50),
    zone_id             INTEGER REFERENCES zone (id),
    country_id          INTEGER REFERENCES country (id),
    PRIMARY KEY (id),
    UNIQUE (code)
);
CREATE TABLE indexation_month
(
    id         SERIAL NOT NULL,
    month_name VARCHAR(50),
    PRIMARY KEY (id)
);
CREATE TABLE referential
(
    id                              SERIAL              NOT NULL,
    code                            VARCHAR(255)        NOT NULL,
    name                            VARCHAR(255)        NOT NULL,
    brand_id                        INTEGER REFERENCES brand (id),
    operating_mode_id               INTEGER REFERENCES operating_mode (id),
    referential_type_id             INTEGER             NOT NULL REFERENCES referential_type (id),
    street                          VARCHAR(100),
    street_complement               VARCHAR(100),
    zip_code                        VARCHAR(10),
    city                            VARCHAR(50),
    country_id                      INTEGER             NOT NULL REFERENCES country (id),
    state_id                        INTEGER REFERENCES states (id),
    region_id                       INTEGER REFERENCES region (id),
    phone                           VARCHAR(255),
    fax                             VARCHAR(255),
    web_site                        VARCHAR(255),
    email                           VARCHAR(255),
    /*company_id                    INTEGER,*/
    currency_id                     INTEGER REFERENCES currency (id),
    location_type_id                INTEGER REFERENCES location_type (id),
    mon_opening_hours               VARCHAR(100),
    tue_opening_hours               VARCHAR(100),
    wed_opening_hours               VARCHAR(100),
    thu_opening_hours               VARCHAR(100),
    fri_opening_hours               VARCHAR(100),
    sat_opening_hours               VARCHAR(100),
    sun_opening_hours               VARCHAR(100),
    opening_date                    TIMESTAMP,
    closing_date                    TIMESTAMP,
    gross_surface                   NUMERIC(20, 2),
    sales_area                      NUMERIC(20, 2),
    full_time_equivalent            NUMERIC(20, 2),
    concept_id                      INTEGER REFERENCES concept (id),
    main_site                       BOOLEAN DEFAULT '0' NOT NULL,
    administrative_code             VARCHAR(255),
    iban                            VARCHAR(34),
    bic                             VARCHAR(11),
    exclude_from_sales_analysis     BOOLEAN DEFAULT '0' NOT NULL,
    exclude_from_financial_analysis BOOLEAN DEFAULT '0' NOT NULL,
    latitude                        NUMERIC(10, 5),
    longitude                       NUMERIC(10, 5),
    qr_code_id                      TEXT,
    qr_code_created_date            TIMESTAMP,
    cost_center                     VARCHAR(30),
    created_by                      TEXT,
    creation_date                   TIMESTAMP,
    last_modified_by                TEXT,
    last_modification_date          TIMESTAMP,
    legal_form                      VARCHAR(50),
    email_invoicing                 VARCHAR(200),
    month_of_closing                INTEGER REFERENCES indexation_month (id),
    equity                          INTEGER,
    vat_exempt                      BOOLEAN,
    vat_number                      VARCHAR(15),
    nace_id                         INTEGER,
    is_deleted                      BOOLEAN DEFAULT '0' NOT NULL,
    comment                         TEXT,
    /*parent_id                     INTEGER,*/
    is_company                      BOOLEAN DEFAULT '0' NOT NULL,
    active                          BOOLEAN,
    PRIMARY KEY (id),
    UNIQUE (code, referential_type_id)
);
CREATE TABLE referential_history
(
    referential_id         INTEGER     NOT NULL REFERENCES referential (id),
    configuration_field_id INTEGER     NOT NULL REFERENCES configuration_field (id),
    date_start             TIMESTAMP   NOT NULL,
    end_date               TIMESTAMP,
    modified_on            TIMESTAMP   NOT NULL,
    comment                VARCHAR(2000),
    status                 VARCHAR(50) NOT NULL,
    modified_by            TEXT        NOT NULL,
    created_by             TEXT        NOT NULL,
    parent_id              INTEGER,
    nace_id                INTEGER,
    month_of_closing       INTEGER REFERENCES indexation_month (id),
    concept_id             INTEGER REFERENCES concept (id),
    location_type_id       INTEGER REFERENCES location_type (id),
    company_id             INTEGER REFERENCES referential (id),
    region_id              INTEGER REFERENCES region (id),
    state_id               INTEGER REFERENCES states (id),
    operating_mode_id      INTEGER REFERENCES operating_mode (id),
    brand_id               INTEGER REFERENCES brand (id),
    list_item_id           INTEGER REFERENCES configuration_field_list_item (id),
    long_text_value        VARCHAR(2000),
    short_text_value       VARCHAR(255),
    yes_no_value           BOOLEAN,
    number_value           NUMERIC(20, 2),
    percentage_value       NUMERIC(7, 4),
    date_time_value        TIMESTAMP,
    email_value            VARCHAR(255),
    PRIMARY KEY (referential_id, configuration_field_id, modified_on)
);
CREATE TABLE configuration_field_value
(
    id                     SERIAL  NOT NULL,
    configuration_field_id INTEGER NOT NULL REFERENCES configuration_field (id),
    short_text_value       VARCHAR(255),
    long_text_value        VARCHAR(2000),
    date_time_value        TIMESTAMP,
    percentage_value       NUMERIC(7, 4),
    number_value           NUMERIC(20, 2),
    yes_no_value           BOOLEAN,
    email_value            VARCHAR(255),
    currency_id            INTEGER REFERENCES currency (id),
    list_item_id           INTEGER REFERENCES configuration_field_list_item (id),
    referential_id         INTEGER REFERENCES referential (id),
    PRIMARY KEY (id)
);
ALTER TABLE referential
    ADD COLUMN company_id INTEGER REFERENCES referential (id);
ALTER TABLE referential
    ADD COLUMN parent_id INTEGER REFERENCES referential (id);
