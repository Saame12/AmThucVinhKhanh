-- Tạo bảng Subscriptions
CREATE TABLE IF NOT EXISTS Subscriptions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GuestId TEXT NOT NULL,
    PaymentCode TEXT NOT NULL,
    StartDate TEXT NOT NULL,
    EndDate TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Active',
    Amount REAL NOT NULL,
    CreatedAt TEXT NOT NULL
);

-- Tạo index để tìm kiếm nhanh
CREATE INDEX IF NOT EXISTS idx_subscriptions_guestid ON Subscriptions(GuestId);
CREATE INDEX IF NOT EXISTS idx_subscriptions_status ON Subscriptions(Status);
