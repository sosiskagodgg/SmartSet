-- ”даление таблиц, если они существуют (в правильном пор€дке из-за зависимостей)
DROP TABLE IF EXISTS UserWorkouts;
DROP TABLE IF EXISTS Workouts;
DROP TABLE IF EXISTS UserParameters;
DROP TABLE IF EXISTS Users;

-- “аблица пользователей
CREATE TABLE Users (
    TelegramId BIGINT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Username VARCHAR(100),
    SubscriptionEndDate TIMESTAMP,
    SubscriptionStatus VARCHAR(50) NOT NULL DEFAULT 'inactive',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- “аблица параметров пользовател€ (только то, что ты сказал)
CREATE TABLE UserParameters (
    TelegramId BIGINT PRIMARY KEY REFERENCES Users(TelegramId) ON DELETE CASCADE,
    Height INTEGER, -- рост в см
    Weight DECIMAL(5,2), -- вес в кг
    BodyFat DECIMAL(4,1), -- процент жира
    Experience VARCHAR(50), -- опыт: beginner/intermediate/advanced
    Goals TEXT -- цели в свободном формате
);

-- “аблица тренировок пользовател€ (по дн€м)
CREATE TABLE UserWorkouts (
    TelegramId BIGINT NOT NULL,
    DayNumber INTEGER NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Exercises JSONB NOT NULL DEFAULT '[]',
    PRIMARY KEY (TelegramId, DayNumber),
    FOREIGN KEY (TelegramId) REFERENCES Users(TelegramId) ON DELETE CASCADE
);

-- “аблица ежедневных тренировок
CREATE TABLE Workouts (
    TelegramId BIGINT NOT NULL,
    Date DATE NOT NULL,
    Exercises JSONB NOT NULL DEFAULT '[]',
    PRIMARY KEY (TelegramId, Date),
    FOREIGN KEY (TelegramId) REFERENCES Users(TelegramId) ON DELETE CASCADE
);

-- »ндексы
CREATE INDEX idx_users_username ON Users(Username);
CREATE INDEX idx_users_subscription ON Users(SubscriptionStatus, SubscriptionEndDate);
CREATE INDEX idx_userworkouts_day ON UserWorkouts(TelegramId, DayNumber);
CREATE INDEX idx_workouts_date ON Workouts(TelegramId, Date);