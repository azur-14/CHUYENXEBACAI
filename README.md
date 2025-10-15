# CHUYENXEBACAI — Volunteer Campaign Management API

Hệ thống backend quản lý **chiến dịch thiện nguyện** & **tình nguyện viên** theo mô hình modular-monolith nhẹ:

* Chiến dịch (campaigns), phiên hoạt động (sessions), đăng ký/duyệt volunteer.
* Điểm danh (QR/MANUAL), đính kèm media.
* Quyên góp/thu–chi, sao kê & đối soát.
* Bài viết/FAQ/Newsletter.
* Audit nhật ký thay đổi.
* JWT Bearer Auth + Swagger.

## 1) Công nghệ

| Thành phần | Công nghệ                                               |
| ---------- | ------------------------------------------------------- |
| Ngôn ngữ   | C# (.NET 8)                                             |
| Framework  | ASP.NET Core Web API                                    |
| CSDL       | PostgreSQL 16+                                          |
| ORM        | Entity Framework Core 9 + Npgsql                        |
| Bảo mật    | JWT (Bearer) + BCrypt                                   |
| Docs       | Swagger (Swashbuckle)                                   |
| Khác       | EFCore.NamingConventions (snake_case), FluentValidation |

## 2) Cấu trúc thư mục

```
CHUYENXEBACAI/
├─ Controllers/                     # API endpoints (Auth, Campaigns, Sessions, Finance, Content, Audit, ...)
├─ Domain/
│  └─ Enum.cs                       # Enum domain (CampaignStatus, SessionShift, …)
├─ Infrastructure/
│  ├─ EF/
│  │  ├─ Models/                    # Entity scaffold từ DB (snake_case)
│  │  ├─ AppDbContext.cs            # DbContext scaffold (partial)
│  │  └─ AppDbContext.Partial.cs    # Bổ sung mapping enum, tweak fluent
│  └─ Json/
│     └─ DateOnlyJsonConverter.cs
├─ Auth/
│  ├─ JwtOptions.cs
│  └─ JwtService.cs
├─ Validation/
│  └─ CreateCampaignDtoValidator.cs
├─ appsettings.json
├─ Program.cs                       # DI + JSON + Swagger + CORS + JWT + DbContext
├─ Dockerfile
├─ docker-compose.yml               # (tuỳ chọn) API + PostgreSQL
└─ sql/
   ├─ 00_schema.sql
   ├─ 01_triggers_views.sql
   └─ 02_seed.sql
```

> Lưu ý: Entities trong `Infrastructure/EF/Models` dùng **snake_case** để match DB.

---

## 3) Yêu cầu môi trường

* .NET SDK **8.0.x**
* PostgreSQL **16+**
* (tuỳ chọn) Docker Desktop (nếu chạy bằng compose)
* pgAdmin/psql để chạy script SQL

Tuyệt — mình viết lại README thật dễ hiểu, kèm **cài Swagger từ A→Z** và hướng dẫn **Authorize (Bearer token)** rõ ràng.


## 4) Cài dependencies (bao gồm **Swagger**)

Chạy trong thư mục dự án (chứa file `.csproj`):

```bash
dotnet add package Npgsql
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package EFCore.NamingConventions

dotnet add package Microsoft.EntityFrameworkCore --version 9.0.9
dotnet add package Microsoft.EntityFrameworkCore.Relational --version 9.0.9
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.9

dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.19
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.14.0

# Swagger (Swashbuckle)
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
# nếu gặp NU1605 với OpenAPI, thêm dòng dưới
dotnet add package Microsoft.OpenApi --version 1.6.14
```

> Nếu trước đó bạn đã cài rồi thì lệnh sẽ báo “đã tồn tại” — không sao.

---

## 5) Tạo database & chạy schema

1. Tạo DB `chuyenxebacai`
2. Chạy lần lượt các script (pgAdmin hoặc `psql`):

```
sql/00_schema.sql
sql/01_triggers_views.sql
sql/02_seed.sql
```

> Nếu thiếu, nhớ bật extension:

```sql
CREATE EXTENSION IF NOT EXISTS pgcrypto;
```

---

## 6) Cấu hình `appsettings.json`

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=chuyenxebacai;Username=app_user;Password=changeme;Include Error Detail=true"
  },
  "Jwt": {
    "Issuer": "cxb",
    "Audience": "cxb",
    "Key": "put-a-very-long-secret-here-change-me",
    "AccessTokenMinutes": 120
  },
  "AllowedHosts": "*"
}
```

> Production: dùng **biến môi trường** thay vì ghi trực tiếp:
> `ConnectionStrings__Default`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__Key`…

---

## 7) Bật **Swagger** trong `Program.cs` (rất ngắn gọn)

Trong phần **services** (trước `builder.Build()`):

```csharp
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CHUYENXEBACAI API",
        Version = "v1",
        Description = "Backend .NET 8 + PostgreSQL (JWT)"
    });

    // Tránh trùng tên schema nếu có DTO trùng tên
    c.CustomSchemaIds(t => t.FullName!.Replace('+', '_'));

    // Map kiểu ngày/giờ đặc biệt (nếu dùng)
    c.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date" });
    c.MapType<TimeOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time" });

    // Cấu hình nút Authorize (Bearer)
    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token: Bearer {token}"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});
```

Trong **pipeline** (sau `var app = builder.Build();`):

```csharp
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CHUYENXEBACAI API v1");
    c.RoutePrefix = "swagger"; // => /swagger
});

app.MapControllers();

---

## 8) Chạy dự án

```bash
dotnet restore
dotnet build
dotnet run
```

Xem cổng ở console, ví dụ:

```
Now listening on: http://localhost:5013
```

Mở **Swagger UI** tại:
`http://localhost:5013/swagger`
(hoặc theo đúng cổng bạn thấy trong console)

---

## 9) **Authorize (Bearer token)** trong Swagger — làm đúng 3 bước

### Bước 1: Đăng ký

* `POST /api/auth/register`

```json
{ "email": "admin@example.com", "password": "P@ssw0rd!", "fullName": "Admin" }
```

### Bước 2: Đăng nhập lấy token

* `POST /api/auth/login`

```json
{ "email": "admin@example.com", "password": "P@ssw0rd!" }
```

* Kết quả:

```json
{ "access_token": "eyJhbGciOi..." }
```

### Bước 3: Nhấn **Authorize** (hình cái khóa ở Swagger)

* Trong ô **Bearer**, dán **đúng định dạng**:

```
Bearer eyJhbGciOi...
```

*(nhớ có chữ “Bearer ” + khoảng trắng)*

* Bấm **Authorize** → **Close**.
  Từ giờ Swagger sẽ tự gửi `Authorization: Bearer <token>` cho mọi request.

---

## 10) Test nhanh vài API

* **Tạo campaign**

```
POST /api/campaigns
(đã Authorize)
{
  "Title": "Mùa Đông Ấm",
  "Description": "Phát quà vùng cao",
  "Location": "Hà Giang",
  "GoalAmount": 50000000
}
```

* **Lấy danh sách campaign**

```
GET /api/campaigns
(đã Authorize)
```

* **Tạo session cho campaign**

```
POST /api/campaigns/{campaignId}/sessions
(đã Authorize)
{
  "Title": "Đợt 1",
  "SessionDate": "2025-11-20",
  "Shift": "MORNING",
  "Status": "PLANNED",
  "Quota": 30
}
```

---

## 11) Lỗi hay gặp & cách xử lý nhanh

* **Không mở được /swagger**

  * Mở đúng cổng theo console (VD `http://localhost:5013/swagger`).
  * Đảm bảo đã có `app.UseSwagger(); app.UseSwaggerUI(...); app.MapControllers();`
  * Thử `http://localhost:<port>/swagger/v1/swagger.json` — nếu JSON OK, UI sẽ chạy.

* **/swagger/v1/swagger.json trả 500**

  * Do trùng tên DTO (VD có 2 `RegisterDto`).
  * Khắc phục: thêm `c.CustomSchemaIds(t => t.FullName!.Replace('+','_'));` và tránh DTO lồng (nested) trùng tên.

* **401 Unauthorized**

  * Chưa bấm **Authorize** hoặc quên tiền tố **Bearer**.
  * Token hết hạn.
  * Sai Issuer/Audience/Key trong `appsettings.json`.

* **NU1605 (package downgrade)**

  * Đồng bộ EF Core:

    ```bash
    dotnet add package Microsoft.EntityFrameworkCore --version 9.0.9
    dotnet add package Microsoft.EntityFrameworkCore.Relational --version 9.0.9
    dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.9
    ```
  * Nâng/bỏ pin `Microsoft.OpenApi` (đề nghị `1.6.14`).

---

## 12) Gợi ý cấu hình khi deploy

* Dùng **HTTPS**, reverse proxy (Nginx/Apache).
* Giới hạn **CORS** theo domain của frontend.
* Không commit `Jwt:Key`/ConnectionStrings prod.
* Xem xét **Refresh Token** và phân quyền role `[Authorize(Roles="ADMIN")]` cho API nhạy cảm.