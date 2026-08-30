# Login API (ASP.NET Core)

หลังบ้านสำหรับระบบ Login/สมัครสมาชิก — API แบบง่ายๆ เชื่อมต่อ SQL Server ผ่าน EF Core

## เทคโนโลยี

- .NET 10 (ASP.NET Core Web API)
- EF Core (SQL Server)
- Password hash ด้วย SHA-256

## โครงสร้าง

```
backend/
├── Controllers/AuthController.cs   # API login + register
├── Data/AppDbContext.cs            # DbContext + mapping ตาราง Users
├── Models/User.cs                  # Entity ผู้ใช้ (ตาราง Users)
├── Program.cs                      # จุดเริ่มต้น (DI, CORS, routing)
├── appsettings.json                # config ทั่วไป (secret อยู่ที่ appsettings.Development.json)
└── appsettings.Development.json    # connection string จริง (exclude ออกจาก git)
```

## วิธีรัน

```
cd backend
dotnet run
```

เซิร์ฟเวอร์จะขึ้นที่ `http://localhost:5285`

## ฐานข้อมูล

ตาราง `Users` ที่ใช้ (สร้างไว้เองที่ฝั่ง DB):

```sql
CREATE TABLE dbo.Users (
    id uniqueidentifier DEFAULT newid() NOT NULL,
    username nvarchar(50) NOT NULL,
    passwordHash nvarchar(255) NOT NULL,
    createdDate datetime2 DEFAULT sysutcdatetime() NULL,
    CONSTRAINT PK_Users PRIMARY KEY (id),
    CONSTRAINT UQ_Users_username UNIQUE (username)
);
```

Connection string อยู่ในไฟล์ `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=master;User Id=sa;Password=xxx;TrustServerCertificate=True;"
}
```

> หมายเหตุ: ไฟล์นี้ถูก gitignore ไม่ขึ้น git ให้สร้างเองเมื่อ clone ไปเครื่องใหม่ หรือใช้ environment variable `ConnectionStrings__DefaultConnection` แทนก็ได้

## API Endpoints

### สมัครสมาชิก

```
POST /api/auth/register
Content-Type: application/json

{ "username": "somchai", "password": "secret123" }
```

| ผลลัพธ์ | Status |
|--------|--------|
| สมัครสำเร็จ | `200 OK` |
| ชื่อผู้ใช้ซ้ำ (มีอยู่แล้ว) | `409 Conflict` |
| กรอกไม่ครบ | `400 Bad Request` |

### เข้าสู่ระบบ

```
POST /api/auth/login
Content-Type: application/json

{ "username": "somchai", "password": "secret123" }
```

| ผลลัพธ์ | Status |
|--------|--------|
| เข้าสู่ระบบสำเร็จ | `200 OK` |
| ผู้ใช้/รหัสไม่ถูกต้อง | `401 Unauthorized` |
| กรอกไม่ครบ | `400 Bad Request` |

## หมายเหตุ

- ยังไม่มี JWT เข้าไว้ — ตอนนี้ login แล้ว client เก็บ `username` ไว้เองฝั่งเดียว (ง่ายๆ ตาม spec)
- เปิด CORS ให้ `http://localhost:4200` (Angular dev server) และโดมิน Cloudflare แล้ว — แก้ได้ที่ `appsettings.json` → `Cors:AllowedOrigins`

## รันด้วย Docker (SQL Server + API)

ใช้ `docker-compose` จะได้ SQL Server ขึ้นมาพร้อมกับ API ใน network เดียวกัน
(API จะสร้างตาราง `Users` ให้อัตโนมัติตอน start ผ่าน EF migration)

```
cd backend
copy .env.example .env      # จริงๆ แล้วแก้ SA_PASSWORD ให้เป็นรหัสที่ต้องการ
docker compose up -d
```

- SQL Server → `localhost:1433` (sa / ตาม `.env`)
- API → `http://localhost` (port 80)

สร้าง image อย่างเดียว (ไม่ต้องรัน):

```
docker build -t loginapi .
```