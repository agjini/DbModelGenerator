ALTER TABLE other DROP CONSTRAINT test_fkey;

ALTER TABLE other ADD COLUMN contract_id INT NOT NULL DEFAULT 0;

ALTER TABLE other
    ADD CONSTRAINT other_contract_id_fkey FOREIGN KEY (contract_id) REFERENCES contract (id);

ALTER TABLE contract
    ALTER COLUMN id TYPE VARCHAR(10);

ALTER TABLE other
    ADD CONSTRAINT other_id_pkey PRIMARY KEY (id);
