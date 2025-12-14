using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using NotesApp.API.DBContext;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Repositories;
using NotesApp.API.Services;
using NotesApp.API.Settings;
using NotesApp.API.Utility;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var services = builder.Services;

// DB
var connectionString =
    Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("NotesDbConnectionString");

services.AddDbContext<NoteDBContext>(options => options.UseNpgsql(connectionString));

// DI
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ITokenHelper, TokenHelper>();

// JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"];

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSettings = jwtSection.Get<JwtSettings>()!;
jwtSettings.Key = jwtKey!;

services.Configure<JwtSettings>(options =>
{
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.Key = jwtSettings.Key;
    options.ExpiresInHours = jwtSettings.ExpiresInHours;
});

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("auth_token", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Swagger
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notes API");
        options.RoutePrefix = "swagger";
    });
}

// FOR RAILWAY (Reverse Proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { }
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();