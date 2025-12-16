using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Semestralka.Infrastructure.Data.Persistence;
using Semestralka.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

const string JwtKey = "THIS_IS_THE_FINAL_TEST_KEY_1234567890_ABCDEF_0987654321";

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CalendarDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    )
);

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

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ShareService>();
builder.Services.AddScoped<SettingsService>();


var app = builder.Build();


app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");

app.Run();


// DODĚLAT SLUŽBY A SERVEROVÉ VALIDACE