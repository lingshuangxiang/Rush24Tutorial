-- 比赛主表
CREATE TABLE matches (
    match_id SERIAL PRIMARY KEY,
    room_id UUID NOT NULL UNIQUE,
    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,
    stages JSONB NOT NULL,          -- 题目信息（JSON存储）
    all_resolved BOOLEAN NOT NULL,
    is_robot_room BOOLEAN NOT NULL DEFAULT false,  -- 新增字段
    red_team_progress JSONB NOT NULL DEFAULT '{}'::jsonb,
    blue_team_progress JSONB NOT NULL DEFAULT '{}'::jsonb,
    battle_mode INT NOT NULL DEFAULT 0, -- 新增对战模式字段
    tournament_slug_name VARCHAR(80) DEFAULT '' -- 锦标赛标识
);

-- 参赛玩家表
CREATE TABLE match_players (
    match_id INT REFERENCES matches(match_id) ON DELETE CASCADE,
    persona_id VARCHAR(40),
    team VARCHAR(5) CHECK (team IN ('RED', 'BLUE')),
    current_index INT,
    previous_score INT,
    current_score INT,
    current_tier VARCHAR(40),
    previous_tier VARCHAR(40),
    is_robot BOOLEAN DEFAULT false,
    battle_result INT DEFAULT 0, -- 比赛结果
    PRIMARY KEY (match_id, persona_id)
);

-- 题目解决状态表
CREATE TABLE resolved_records (
    record_id SERIAL PRIMARY KEY,
    match_id INT REFERENCES matches(match_id) ON DELETE CASCADE,
    question_index INT NOT NULL,     -- 对应stages数组索引
    resolved_team INT NOT NULL,      -- 0=red, 1=blue
    resolved BOOLEAN NOT NULL,
    resolved_persona_id VARCHAR(40),
    resolved_time DOUBLE PRECISION DEFAULT 0,
    cost_time DOUBLE PRECISION DEFAULT 0
);

-- 玩家信息表
CREATE TABLE player_info (
    persona_id VARCHAR(40) PRIMARY KEY,
    total_resolved INT DEFAULT 0,
    total_time BIGINT DEFAULT 0,
    current_score INT DEFAULT 0,
    current_tier VARCHAR(40) DEFAULT '',
    highest_score INT DEFAULT 0,
    highest_tier VARCHAR(40) DEFAULT ''
);

-- 玩家体力表
CREATE TABLE player_vitality (
    persona_id VARCHAR(40) PRIMARY KEY,
    last_update_time TIMESTAMPTZ
);

-- 订阅表
CREATE TABLE wechat_notify (
    wechat_notify_id SERIAL PRIMARY KEY,
    template_id VARCHAR(255) NOT NULL,
    touser VARCHAR(255) NOT NULL,
    page VARCHAR(40) DEFAULT '',
    data JSONB NOT NULL DEFAULT '{}'::jsonb,
    miniprogram_state VARCHAR(40) DEFAULT 'normal',
    lang VARCHAR(40) DEFAULT 'zh_CN',
    notified BOOLEAN NOT NULL DEFAULT false, -- 是否已经通知
    notify_time TIMESTAMPTZ NOT NULL,
    slug_name VARCHAR(255) DEFAULT '' NOT NULL
);

-- 创建索引优化查询
CREATE INDEX idx_players ON match_players(persona_id);
CREATE INDEX idx_resolved_persona ON resolved_records(resolved_persona_id);

-- 创建索引优化排序性能
CREATE INDEX idx_matches_time ON matches(start_time DESC);
