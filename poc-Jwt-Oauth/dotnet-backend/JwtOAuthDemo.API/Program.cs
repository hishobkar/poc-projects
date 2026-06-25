using System.Text;
using System.Security.Cryptography;
using JwtOAuthDemo.API.Models;
using JwtOAuthDemo.API.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSettingsSection.Get<JwtSettings>() ?? throw new InvalidOperationException("JWT Settings not configured");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.Configure<JwtSettings>(jwtSettingsSection);

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["OAuth:Google:ClientId"] ?? string.Empty;
    options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"] ?? string.Empty;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["OAuth:Facebook:AppId"] ?? string.Empty;
    options.AppSecret = builder.Configuration["OAuth:Facebook:AppSecret"] ?? string.Empty;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("JwtOAuthDemoDb"));

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedData(dbContext);
}

app.Run();

async Task SeedData(ApplicationDbContext context)
{
    if (!context.Users.Any())
    {
        var testUser = new JwtOAuthDemo.Core.Models.User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("Test123!"))),
            Provider = "local",
            Roles = new[] { "User", "Admin" }
        };
        context.Users.Add(testUser);
        await context.SaveChangesAsync();
    }
}