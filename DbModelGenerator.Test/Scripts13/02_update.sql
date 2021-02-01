ALTER TABLE user_grid_state ADD IF NOT EXISTS shared_view_id INT NULL;
ALTER TABLE user_grid_state DROP CONSTRAINT IF EXISTS  user_grid_state_fk;
ALTER TABLE user_grid_state ADD CONSTRAINT  user_grid_state_fk FOREIGN KEY (shared_view_id) REFERENCES user_grid_state(id);
ALTER TABLE user_grid_state ADD IF NOT EXISTS is_sharded BOOLEAN NOT NULL;