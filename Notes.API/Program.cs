using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Modules.Auth.Repositories;
using NotesApp.API.Modules.Auth.Services;
using NotesApp.API.Modules.Auth.Interfaces.Repositories;
using NotesApp.API.Modules.Auth.Settings;
using NotesApp.API.Modules.Auth.Utility;
using NotesApp.API.Infrastructure.DBContext;
using NotesApp.API.Common.Middleware;
using NotesApp.API.Modules.Notes.Interfaces.Repositories;
using NotesApp.API.Modules.Notes.Interfaces.Services;
using NotesApp.API.Modules.Notes.Repositories;
using NotesApp.API.Modules.Notes.Services;
using NotesApp.API.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var services = builder.Services;

var connectionString =
    Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("NotesDbConnectionString");

services.AddDbContext<NoteDBContext>(options => options.UseNpgsql(connectionString));

services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRoleRepository, RoleRepository>();
services.AddScoped<ITokenProvider, TokenProvider>();
services.AddScoped<IHashProvider, HashProvider>();

services.AddScoped<INoteService, NoteService>();
services.AddScoped<INoteRepository, NoteRepository>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins(
                "http://localhost:3000",
                "https://generous-mercy-dev.up.railway.app"
            );
    });
});

services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notes API");
        options.RoutePrefix = "swagger";
    });
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { }
});

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var roleRepo = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
    var hashProvider = scope.ServiceProvider.GetRequiredService<IHashProvider>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    await DataSeeder.SeedRolesAsync(roleRepo);
    await DataSeeder.SeedAdminUserAsync(userRepo, roleRepo, hashProvider, configuration);
}

app.Run();