using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Semestralka.Data;
using Semestralka.Services;
using System.Text; 


var builder = WebApplication.CreateBuilder(args);

const string JwtKey = "THIS_IS_THE_FINAL_TEST_KEY_1234567890_ABCDEF_0987654321";

// MVC + API
builder.Services.AddControllersWithViews();

// DATABASE
builder.Services.AddDbContext<CalendarDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// JWT AUTH
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "userid"
        };
    });

builder.Services.AddAuthorization();

// BACKGROUND SERVICES
builder.Services.AddHostedService<NotificationService>();

// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SESSION
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// SWAGGER UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Calendar API v1");
    c.DocumentTitle = "Kalendář – API dokumentace";
});

// MIDDLEWARE PIPELINE
app.UseStaticFiles();
app.UseRouting();
app.UseSession();       // MUSÍ být před UseAuthentication a před UseAuthorization
app.UseAuthentication();
app.UseAuthorization();


// ROUTES
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");


app.Run();
