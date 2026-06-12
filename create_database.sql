-- LAN Group Chat Web Application Database
-- PostgreSQL script for psql

CREATE DATABASE group_chat_app;

\connect group_chat_app

CREATE TABLE participants (
    participant_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    display_name VARCHAR(50) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE chat_groups (
    group_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    group_name VARCHAR(100) NOT NULL,
    description TEXT,
    created_by BIGINT NOT NULL REFERENCES participants(participant_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE group_members (
    group_member_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    group_id BIGINT NOT NULL REFERENCES chat_groups(group_id) ON DELETE CASCADE,
    participant_id BIGINT NOT NULL REFERENCES participants(participant_id) ON DELETE CASCADE,
    joined_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (group_id, participant_id)
);

CREATE TABLE messages (
    message_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    group_id BIGINT NOT NULL REFERENCES chat_groups(group_id) ON DELETE CASCADE,
    sender_id BIGINT NOT NULL REFERENCES participants(participant_id) ON DELETE CASCADE,
    message_text TEXT,
    message_type VARCHAR(20) NOT NULL DEFAULT 'text',
    sent_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_message_type CHECK (message_type IN ('text', 'image', 'file')),
    CONSTRAINT chk_message_content CHECK (
        message_text IS NOT NULL OR message_type IN ('image', 'file')
    )
);

CREATE TABLE attachments (
    attachment_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    message_id BIGINT NOT NULL REFERENCES messages(message_id) ON DELETE CASCADE,
    original_file_name VARCHAR(255) NOT NULL,
    stored_file_name VARCHAR(255) NOT NULL,
    file_path TEXT NOT NULL,
    file_type VARCHAR(100) NOT NULL,
    file_size BIGINT NOT NULL,
    uploaded_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_chat_groups_created_by ON chat_groups(created_by);
CREATE INDEX idx_group_members_group_id ON group_members(group_id);
CREATE INDEX idx_group_members_participant_id ON group_members(participant_id);
CREATE INDEX idx_messages_group_id_sent_at ON messages(group_id, sent_at);
CREATE INDEX idx_messages_sender_id ON messages(sender_id);
CREATE INDEX idx_attachments_message_id ON attachments(message_id);
