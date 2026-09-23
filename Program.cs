using ClothingErp.Api.Data;
using ClothingErp.Api.Interfaces;
using ClothingErp.Api.Repositories;
using ClothingErp.Api.Services;

using CLOTHS_ERP.API.Interfaces;
using CLOTHS_ERP.API.Options;
using CLOTHS_ERP.API.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.Text;

const string CorsPolicyName = "AllowAngular";

var builder = WebApplication.CreateBuilder(args);

// ===============================================================
// Configuration
// ===============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

var jwtSection =
    builder.Configuration.GetSection("JwtSettings");

var jwtIssuer =
    jwtSection["Issuer"]
    ?? throw new InvalidOperationException(
        "JwtSettings:Issuer is not configured.");

var jwtAudience =
    jwtSection["Audience"]
    ?? throw new InvalidOperationException(
        "JwtSettings:Audience is not configured.");

var jwtSecretKey =
    jwtSection["SecretKey"]
    ?? throw new InvalidOperationException(
        "JwtSettings:SecretKey is not configured.");

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>();

if (allowedOrigins is null || allowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "Cors:AllowedOrigins is not configured.");
}

// ===============================================================
// Controllers
// ===============================================================

builder.Services.AddControllers();

// ===============================================================
// Database
// ===============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ===============================================================
// Menu Options
// ===============================================================

builder.Services
    .AddOptions<MenuOptions>()
    .Bind(builder.Configuration.GetSection(MenuOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ===============================================================
// Repository
// ===============================================================

builder.Services.AddScoped(
    typeof(IRepository<>),
    typeof(Repository<>));

// ===============================================================
// Infrastructure Services
// ===============================================================

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ===============================================================
// Business Services
// ===============================================================

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IMenuService, MenuService>();

// ===============================================================
// JWT Authentication
// ===============================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecretKey))
            };
    });

builder.Services.AddAuthorization();

// ===============================================================
// Swagger
// ===============================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===============================================================
// CORS
// ===============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ===============================================================
// Build Application
// ===============================================================

var app = builder.Build();

// ===============================================================
// Database Seeder
// ===============================================================

using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    var hasher =
        scope.ServiceProvider
            .GetRequiredService<IPasswordHasher>();

    DbSeeder.Seed(db, hasher);
}

// ===============================================================
// HTTP Pipeline
// ===============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();