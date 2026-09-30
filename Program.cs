using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

using ClothingErp.Api.Data;
using ClothingErp.Api.Interfaces;
using ClothingErp.Api.Middleware;
using ClothingErp.Api.Models;
using ClothingErp.Api.Repositories;
using ClothingErp.Api.Services;

using CLOTHS_ERP.API.Interfaces;
using CLOTHS_ERP.API.Options;
using CLOTHS_ERP.API.Repositories;
using CLOTHS_ERP.API.Services;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

const string CorsPolicyName = "AllowAngular";

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Configuration
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

var jwt =
    builder.Configuration.GetSection("JwtSettings");

var secret =
    jwt["SecretKey"]
    ?? throw new InvalidOperationException(
        "JwtSettings:SecretKey is not configured.");

var issuer =
    jwt["Issuer"]
    ?? throw new InvalidOperationException(
        "JwtSettings:Issuer is not configured.");

var audience =
    jwt["Audience"]
    ?? throw new InvalidOperationException(
        "JwtSettings:Audience is not configured.");

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? new[]
    {
        "http://localhost:4200"
    };


// ============================================================
// Controllers
// ============================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });


// ============================================================
// API Validation Response
// ============================================================

builder.Services.Configure<ApiBehaviorOptions>(
    options =>
    {
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(
                new ClothingErp.Api.Dtos.ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
    });


// ============================================================
// Swagger
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Clothing ERP API",
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter your JWT token. Example: Bearer {token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});


// ============================================================
// Entity Framework / SQL Server
// ============================================================

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseSqlServer(connectionString));


// ============================================================
// ASP.NET Identity
// ============================================================

builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


// ============================================================
// JWT Authentication
// ============================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = issuer,
                ValidAudience = audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secret)),

                ClockSkew =
                    TimeSpan.FromMinutes(1)
            };

        // Do not query Users.IsActive here.
        // The current database does not contain that column.
        // JWT validation itself is handled above.
    });


// ============================================================
// Authorization
// ============================================================

builder.Services.AddAuthorization();


// ============================================================
// FluentValidation
// ============================================================

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();


// ============================================================
// Menu Configuration
// ============================================================

builder.Services
    .AddOptions<MenuOptions>()
    .Bind(
        builder.Configuration.GetSection(
            MenuOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();


// ============================================================
// Application Services
// ============================================================

builder.Services.AddScoped(
    typeof(IRepository<>),
    typeof(Repository<>));

builder.Services.AddScoped<
    IPasswordHasher,
    PasswordHasher>();

builder.Services.AddScoped<
    ITokenService,
    TokenService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IMasterDataService,
    MasterDataService>();

builder.Services.AddScoped<EcommerceService>();

builder.Services.AddScoped<
    IPaymentGateway,
    PaymentGatewayService>();


// ============================================================
// Menu Repository + Service
// ============================================================

builder.Services.AddScoped<
    IMenuRepository,
    MenuRepository>();

builder.Services.AddScoped<
    IMenuService,
    MenuService>();


// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            CorsPolicyName,
            policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });


// ============================================================
// Rate Limiting
// ============================================================

builder.Services.AddRateLimiter(
    options =>
    {
        options.AddFixedWindowLimiter(
            "api",
            limiter =>
            {
                limiter.PermitLimit = 120;

                limiter.Window =
                    TimeSpan.FromMinutes(1);

                limiter.QueueLimit = 0;
            });
    });


// ============================================================
// Build Application
// ============================================================

var app = builder.Build();


// ============================================================
// Database Seeder
// ============================================================

// Seeder intentionally disabled.
// Existing database and Stored Procedures are being used.


// ============================================================
// HTTP Pipeline
// ============================================================

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors(CorsPolicyName);

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers()
    .RequireRateLimiting("api");

app.Run();