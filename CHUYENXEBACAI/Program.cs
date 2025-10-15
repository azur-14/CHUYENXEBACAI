using CHUYENXEBACAI.Infrastructure.EF;
using CHUYENXEBACAI.Domain;
using CHUYENXEBACAI.Auth;
using CHUYENXEBACAI.Infrastructure.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;

using Npgsql;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// 1) Controllers + JSON
// ---------------------------
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

// ---------------------------
// 2) Swagger (single place)
// ---------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CHUYENXEBACAI API",
        Version = "v1",
        Description = "Backend .NET 8 + PostgreSQL (JWT)"
    });

    // Tránh đụng trùng schemaId giữa các lớp trùng tên khác namespace
    c.CustomSchemaIds(t => t.FullName!.Replace('+', '_'));

    // Map các kiểu không mặc định
    c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
    c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });

    // JWT trên Swagger
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token: Bearer {token}"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
});


// ---------------------------
// 3) PostgreSQL + enum mapping
// ---------------------------
var cs = builder.Configuration.GetConnectionString("Default")
         ?? "Host=localhost;Port=5432;Database=chuyenxebacai;Username=app_user;Password=changeme;Include Error Detail=true";

var dsb = new NpgsqlDataSourceBuilder(cs);
dsb.MapEnum<UserStatus>("user_status_enum");
dsb.MapEnum<AppReviewStatus>("app_review_status_enum");
dsb.MapEnum<RegistrationStatus>("registration_status_enum");
dsb.MapEnum<CampaignStatus>("campaign_status_enum");
dsb.MapEnum<SessionStatus>("session_status_enum");
dsb.MapEnum<SessionShift>("session_shift_enum");
dsb.MapEnum<CheckinMethod>("checkin_method_enum");
dsb.MapEnum<CheckinStatus>("checkin_status_enum");
dsb.MapEnum<DonationGateway>("donation_gateway_enum");
dsb.MapEnum<DonationStatus>("donation_status_enum");
dsb.MapEnum<FundSource>("fund_source_enum");
dsb.MapEnum<FundStatus>("fund_status_enum");
dsb.MapEnum<BankImportSource>("bank_import_source_enum");
dsb.MapEnum<ReconcileDecision>("reconcile_decision_enum");
dsb.MapEnum<Currency>("currency_enum");
dsb.MapEnum<ChangeAction>("change_action_enum");
dsb.MapEnum<PostStatus>("post_status_enum");

var dataSource = dsb.Build();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(dataSource)
       .UseSnakeCaseNamingConvention());

// ---------------------------
// 4) JWT Auth
// ---------------------------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IJwtService, JwtService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ---------------------------
// 5) CORS (dev)
// ---------------------------
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ---------------------------
// Pipeline
// ---------------------------
app.UseCors();

// Nếu bạn muốn Swagger luôn bật:
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CHUYENXEBACAI API v1");
    c.RoutePrefix = "swagger";   // => /swagger
});

// Nếu đang chạy HTTPS profile, bật redirect; còn đang chạy HTTP (5013) có thể tắt dòng dưới
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
