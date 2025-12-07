using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotesApp.API.DBContext;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Repositories;
using NotesApp.API.Services;
using NotesApp.API.Settings;
using NotesApp.API.Utility;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddDbContext<NoteDBContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("NotesDbConnectionString")));

services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IUserService, UserService>();

services.AddScoped<IUserRepository, UserRepository>();

services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
services.AddScoped<ITokenHelper, TokenHelper>();

services.AddControllers();
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    JwtSettings? jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
    options.TokenValidationParameters = new TokenValidationParameters
    {

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(jwtSettings.Key)
            ),
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
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notes API");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
