using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MobyPark_api.Services.ParkingLot;



public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // EF Core (PostgreSQL)
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Password hasher and auth service.
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ILicenseplateService, LicenseplateService>();
        builder.Services.AddScoped<ISessionService, SessionService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddScoped<IParkingLotService, ParkingLotService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        


        // JWT authentication.
        var key = builder.Configuration["Jwt:Key"] ?? "dev-only-change-me";
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,    // use "role" if you emitted a custom claim
                    NameClaimType = ClaimTypes.Name
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"));
            options.AddPolicy("UserOrAdmin", p => p.RequireRole("User", "ADMIN"));
        });

        // Swagger with JWT Auth support
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });

            // Add the JWT Bearer definition
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your token.\nExample: Bearer 12345abcdef"
            });

            // Require JWT token for all endpoints by default
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        // app.UseHttpsRedirection(); // comment for http local tests
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Auto-apply migrations.
        // using (var scope = app.Services.CreateScope())
        // {
        //     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //     if (Environment.GetEnvironmentVariable("IsXUnitTesting") != "True")
        //         db.Database.Migrate();
        // }

        app.Run();
    }
}


