ALTER TABLE IF EXISTS sys_translation RENAME TO translation;
ALTER TABLE IF EXISTS translation RENAME COLUMN "code" To "id";