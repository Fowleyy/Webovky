using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Semestralka.Infrastructure.Data.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

const string JwtKey = "THIS_IS_THE_FINAL_TEST_KEY_1234567890_ABCDEF_0987654321";

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();

// =======================
// DATABASE (Infrastructure)
// =======================
builder.Services.AddDbContext<CalendarDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    )
);

// =======================
// JWT AUTH
// =======================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "userid"
        };
    });

builder.Services.AddAuthorization();

// =======================
// SESSION
// =======================
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// =======================
// SWAGGER
// =======================
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();


// =======================
// MIDDLEWARE PIPELINE
// =======================
app.UseStaticFiles();
app.UseRouting();

app.UseSession();      // MUSÍ být před Auth
app.UseAuthentication();
app.UseAuthorization();

// =======================
// ROUTES
// =======================
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");

app.Run();
