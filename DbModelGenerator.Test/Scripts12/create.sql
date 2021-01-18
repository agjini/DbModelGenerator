
CREATE TABLE tenant_saml
(
    tenant_id                  VARCHAR(128)   NOT NULL REFERENCES tenant (id),
    is_enabled                 BOOLEAN        NOT NULL DEFAULT False,
    identity_provider_metadata VARCHAR(10000) NOT NULL,
    default_group_id           INTEGER                 DEFAULT NULL,
    default_culture_id         CHAR(5)                 DEFAULT NULL
);

ALTER TABLE tenant_saml ADD PRIMARY KEY (tenant_id);

ALTER TABLE tenant_saml RENAME COLUMN tenant_id TO new_tenant_id;