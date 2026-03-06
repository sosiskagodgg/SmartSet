-- DROP старых таблиц (порядок с учётом FK)
DROP TABLE IF EXISTS program_day_exercises CASCADE;
DROP TABLE IF EXISTS program_days CASCADE;
DROP TABLE IF EXISTS user_programs CASCADE;
DROP TABLE IF EXISTS exercise_sets CASCADE;
DROP TABLE IF EXISTS workout_exercises CASCADE;
DROP TABLE IF EXISTS workouts CASCADE;
DROP TABLE IF EXISTS program_templates CASCADE;
DROP TABLE IF EXISTS user_custom_exercises CASCADE;
DROP TABLE IF EXISTS exercise_muscles CASCADE;
DROP TABLE IF EXISTS exercise_cardio_params CASCADE;
DROP TABLE IF EXISTS exercise_strength_params CASCADE;
DROP TABLE IF EXISTS exercises CASCADE;
DROP TABLE IF EXISTS user_parameters CASCADE;
DROP TABLE IF EXISTS muscles CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- =====================================================
-- USERS
-- =====================================================
CREATE TABLE users (
    id BIGINT PRIMARY KEY,  -- Это и есть Telegram ID
    username TEXT,
    first_name TEXT,
    last_name TEXT,
    registered_at TIMESTAMPTZ DEFAULT now(),
    last_activity_at TIMESTAMPTZ
);

CREATE INDEX idx_users_username ON users(username);

-- =====================================================
-- USER PARAMETERS
-- =====================================================
CREATE TABLE user_parameters (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    weight_kg NUMERIC(5,2),
    height_cm NUMERIC(5,2),
    birth_date DATE,
    gender TEXT CHECK (gender IN ('male', 'female', 'other')),
    
    activity_level TEXT CHECK (activity_level IN ('sedentary', 'light', 'moderate', 'very', 'extra')),
    experience_level TEXT CHECK (experience_level IN ('beginner', 'intermediate', 'advanced')),
    
    fitness_goals TEXT[],
    
    notes TEXT,
    recorded_at TIMESTAMPTZ DEFAULT now(),
    is_current BOOLEAN DEFAULT true
);

CREATE INDEX idx_user_parameters_user_id ON user_parameters(user_id);
CREATE INDEX idx_user_parameters_recorded_at ON user_parameters(recorded_at);
CREATE INDEX idx_user_parameters_is_current ON user_parameters(is_current);

-- =====================================================
-- MUSCLES
-- =====================================================
CREATE TABLE muscles (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    stamina REAL DEFAULT 100,
    strength REAL DEFAULT 100,
    percentage_of_recovery REAL DEFAULT 100,
    recovery_time INTERVAL,
    user_id BIGINT REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX idx_muscles_user_id ON muscles(user_id);

-- =====================================================
-- EXERCISES
-- =====================================================
CREATE TABLE exercises (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    met DOUBLE PRECISION,
    category TEXT NOT NULL,  -- Strength, Cardio, Stretching
    is_custom BOOLEAN DEFAULT false,
    user_id BIGINT REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ DEFAULT now()
);

CREATE INDEX idx_exercises_name ON exercises(name);
CREATE INDEX idx_exercises_user_id ON exercises(user_id);
CREATE INDEX idx_exercises_category ON exercises(category);

-- =====================================================
-- EXERCISE MUSCLES (связь многие-ко-многим)
-- =====================================================
CREATE TABLE exercise_muscles (
    exercise_id INT NOT NULL REFERENCES exercises(id) ON DELETE CASCADE,
    muscle_id INT NOT NULL REFERENCES muscles(id) ON DELETE CASCADE,
    is_primary BOOLEAN DEFAULT false,
    intensity REAL DEFAULT 100,
    PRIMARY KEY (exercise_id, muscle_id)
);

-- =====================================================
-- PROGRAM TEMPLATES
-- =====================================================
CREATE TABLE program_templates (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT false,
    created_at TIMESTAMPTZ DEFAULT now(),
    start_date DATE,
    end_date DATE
);

CREATE INDEX idx_program_templates_user_id ON program_templates(user_id);
CREATE INDEX idx_program_templates_is_active ON program_templates(is_active);

-- =====================================================
-- PROGRAM DAYS
-- =====================================================
CREATE TABLE program_days (
    id SERIAL PRIMARY KEY,
    program_id INT NOT NULL REFERENCES program_templates(id) ON DELETE CASCADE,
    day_number INT NOT NULL,
    day_name TEXT,
    is_rest_day BOOLEAN DEFAULT false
);

CREATE INDEX idx_program_days_program_id ON program_days(program_id);

-- =====================================================
-- PROGRAM DAY EXERCISES
-- =====================================================
CREATE TABLE program_day_exercises (
    id SERIAL PRIMARY KEY,
    program_day_id INT NOT NULL REFERENCES program_days(id) ON DELETE CASCADE,
    exercise_id INT NOT NULL REFERENCES exercises(id) ON DELETE CASCADE,
    order_number INT NOT NULL,
    sets INT,
    reps_min INT,
    reps_max INT,
    reps_text TEXT,
    weight NUMERIC(6,2),
    weight_unit TEXT,
    duration INT,  -- в секундах
    distance_m NUMERIC(8,2),
    notes TEXT
);

CREATE INDEX idx_program_day_exercises_day ON program_day_exercises(program_day_id);
CREATE INDEX idx_program_day_exercises_exercise ON program_day_exercises(exercise_id);

-- =====================================================
-- WORKOUTS
-- =====================================================
CREATE TABLE workouts (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    started_at TIMESTAMPTZ DEFAULT now(),
    ended_at TIMESTAMPTZ,
    notes TEXT,
    status TEXT DEFAULT 'active' CHECK (status IN ('active', 'completed', 'cancelled')),
    program_day_id INT REFERENCES program_days(id) ON DELETE SET NULL
);

CREATE INDEX idx_workouts_user_id ON workouts(user_id);
CREATE INDEX idx_workouts_status ON workouts(status);
CREATE INDEX idx_workouts_started_at ON workouts(started_at);

-- =====================================================
-- WORKOUT EXERCISES
-- =====================================================
CREATE TABLE workout_exercises (
    id BIGSERIAL PRIMARY KEY,
    workout_id BIGINT NOT NULL REFERENCES workouts(id) ON DELETE CASCADE,
    exercise_id INT NOT NULL REFERENCES exercises(id) ON DELETE RESTRICT,
    order_number INT NOT NULL,
    notes TEXT
);

CREATE INDEX idx_workout_exercises_workout ON workout_exercises(workout_id);
CREATE INDEX idx_workout_exercises_exercise ON workout_exercises(exercise_id);

-- =====================================================
-- EXERCISE SETS
-- =====================================================
CREATE TABLE exercise_sets (
    id BIGSERIAL PRIMARY KEY,
    workout_exercise_id BIGINT NOT NULL REFERENCES workout_exercises(id) ON DELETE CASCADE,
    set_number INT NOT NULL,
    reps INT,
    weight NUMERIC(6,2),
    duration_seconds INT,
    distance_meters NUMERIC(8,2),
    is_completed BOOLEAN DEFAULT true,
    notes TEXT,
    completed_at TIMESTAMPTZ DEFAULT now()
);

CREATE INDEX idx_exercise_sets_workout_exercise ON exercise_sets(workout_exercise_id);
CREATE INDEX idx_exercise_sets_completed_at ON exercise_sets(completed_at);

-- =====================================================
-- USER CUSTOM EXERCISES (для обратной совместимости)
-- =====================================================
CREATE TABLE user_custom_exercises (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    base_exercise_id INT REFERENCES exercises(id) ON DELETE SET NULL,
    custom_name TEXT,
    params JSONB,
    created_at TIMESTAMPTZ DEFAULT now()
);

CREATE INDEX idx_user_custom_exercises_user_id ON user_custom_exercises(user_id);

-- =====================================================
-- СТАРЫЕ ТАБЛИЦЫ (можно удалить если не нужны)
-- =====================================================
-- exercise_strength_params
-- exercise_cardio_params
-- user_programs