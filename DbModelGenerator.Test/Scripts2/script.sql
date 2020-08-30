
CREATE              
    
            
    
    table                   "brand"    (
id          SERIAL       NOT NULL,
name        VARCHAR(50)  NOT NULL,logo        VARCHAR(200),    archived    BOOLEAN DEFAULT '0',
    
    
    
    
    
    color       VARCHAR(100) NOT NULL,
    external_id
        
        
        VARCHAR(500),
    
    
    
    PRIMARY KEY (id),
    UNIQUE (name)
);

  CREate   TABLE bbbb
(
    id          SERIAL       NOT    NULL,
    name        VARCHAR(50)    not NULL,
    logo        VARCHAR(200),
    PRIMARY KEY (id)
);