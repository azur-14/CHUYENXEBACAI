````markdown
# CHUYENXEBACAI — Volunteer Campaign Management API

## Giới thiệu đề tài
**CHUYENXEBACAI** là hệ thống quản lý các **chiến dịch thiện nguyện** và **tình nguyện viên**, cho phép:
- Quản lý chiến dịch, phiên hoạt động (sessions) và tình nguyện viên.
- Điểm danh QR code theo ngày, theo ca.
- Theo dõi quyên góp, tiến độ đạt chỉ tiêu và đối chiếu sao kê.
- Quản lý phân công nhiệm vụ, theo dõi đánh giá hoàn thành.
- Quản trị hệ thống (admin) mở, đóng điểm danh và giám sát toàn bộ chiến dịch.

Ứng dụng được xây dựng theo **mô hình phân lớp**, hướng đến **mở rộng – bảo trì dễ dàng** và sẵn sàng kết nối với mobile/web frontend.

---

## Công nghệ sử dụng

| Thành phần | Công nghệ |
|-------------|------------|
| **Ngôn ngữ** | C# (.NET 8.0) |
| **Framework** | ASP.NET Core Web API |
| **CSDL** | PostgreSQL |
| **ORM** | Entity Framework Core 9.0 + NamingConventions |
| **Bảo mật** | JWT Authentication + BCrypt Password Hashing |
| **Kiểm tra dữ liệu** | FluentValidation |
| **Triển khai** | Docker + Docker Compose |
| **Môi trường phát triển** | Visual Studio 2022 / VS Code |

---

## Cấu trúc thư mục

```plaintext
CHUYENXEBACAI/
│
├── Controllers/                # Các controller xử lý request HTTP
│   ├── AuthController.cs
│   ├── AuditController.cs
│   ├── CampaignsController.cs
│   ├── SessionsController.cs
│   └── ...
│
├── Domain/                     # Lớp miền (Entities, Enum, Models)
│   ├── Enum.cs
│   └── ...
│
├── Infrastructure/             # Lớp truy cập dữ liệu, cấu hình EF
│   ├── AppDbContext.cs
│   └── ...
│
├── Auth/                       # Cấu hình JWT, xác thực
│   ├── JwtService.cs
│   ├── JwtOptions.cs
│   └── ...
│
├── Dto/                        # Data Transfer Objects (input/output)
│
├── appsettings.json            # Cấu hình ứng dụng (DB, JWT, Logging, ...)
├── Program.cs                  # Entry point (Main)
├── Dockerfile                  # Docker build instructions
└── CHUYENXEBACAI.csproj        # File cấu hình project
````

---

## Cách chạy dự án

### 1. Cấu hình cơ sở dữ liệu PostgreSQL

Tạo cơ sở dữ liệu (ví dụ: `chuyenxebacai_db`) và cập nhật chuỗi kết nối trong `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=chuyenxebacai_db;Username=postgres;Password=yourpassword"
}
```

---

### 2. Chạy lệnh khởi tạo database

Trong thư mục dự án:

```bash
dotnet ef database update
```

*(Nếu chưa có EF CLI, cài bằng: `dotnet tool install --global dotnet-ef`)*

---

### 3. Chạy API

```bash
dotnet run
```

Ứng dụng mặc định chạy tại:

```
http://localhost:5000
https://localhost:5001
```

---

### 4. Dùng Docker (tùy chọn)

Build và chạy container:

```bash
docker build -t chuyenxebacai-api .
docker run -d -p 8080:8080 chuyenxebacai-api
```

---

## Ghi chú thêm

* Khi thay đổi model → chạy `dotnet ef migrations add <TênMigration>` rồi `dotnet ef database update`.
