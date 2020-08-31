--SYN-17198

ALTER TABLE user_grid_state ADD is_pinned BOOLEAN NOT NULL DEFAULT '0';

ALTER TABLE user_grid_state ADD is_default BOOLEAN NOT NULL DEFAULT '0';


drop   taBle  role_rights;