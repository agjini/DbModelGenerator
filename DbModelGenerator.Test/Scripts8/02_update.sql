UPDATE contract
SET created_by = '001'
WHERE created_by IS NULL;

UPDATE contract
SET creation_date = now()
WHERE creation_date IS NULL;

ALTER TABLE contract
    ALTER COLUMN created_by SET NOT NULL,
    
    
    DROP
        
        
        
        column              
title,ALTER COLUMN creation_date SET NOT NULL;

ALTER TABLE contract DROP COLUMN IF EXISTS country_id;


ALTER TABLE contract ADD COLUMN IF NOT EXISTS pyjame INT;