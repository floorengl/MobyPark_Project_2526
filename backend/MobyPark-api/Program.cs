using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MobyPark_api.Data.Repositories;
using MobyPark_api.Services;
using MobyPark_api.Services.ParkingLot;



public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // EF Core (PostgreSQL)
        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            var cs = builder.Configuration.GetConnectionString("DefaultConnection")!;

            // If NOT running inside Docker, use localhost + published port
            if (!File.Exists("/.dockerenv"))
                cs = cs.Replace("Host=db", "Host=localhost").Replace("Port=5432", "Port=5434");

            opt.UseNpgsql(cs);
        });


        // Services
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ILicenseplateService, LicenseplateService>();
        builder.Services.AddScoped<ISessionService, SessionService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddScoped<IParkingLotService, ParkingLotService>();
        builder.Services.AddScoped<IVehicleService, VehicleService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IDiscountService, DiscountService>();
        builder.Services.AddScoped<IPricingService, PricingService>();
        // Repositories
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        builder.Services.AddScoped<ILicenseplateRepository, LicenseplateRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
        builder.Services.AddScoped<IParkingLotRepository, ParkingLotRepository>();
        builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();


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

        // If started only for migrations, run them and exit BEFORE app.Run()
        var isTesting = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing"
                     || Environment.GetEnvironmentVariable("IsXUnitTesting") == "True";

        if (!isTesting)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var db = services.GetRequiredService<AppDbContext>();

                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        logger.LogInformation("Attempting to run migrations...");
                        db.Database.Migrate();
                        logger.LogInformation("Migrations applied successfully.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"Migration attempt {i + 1} failed: {ex.Message}");
                        if (i == 4) throw;
                        Thread.Sleep(3000);
                    }
                }
            }
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        // Conditionally enable HTTPS redirection
        var useHttps = string.Equals(
            Environment.GetEnvironmentVariable("USE_HTTPS"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (useHttps)
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Auto-apply migrations.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Delete any existing admin rows (just in case there are duplicates).
            var existingAdmins = db.Users.Where(u => u.Username == "InitialAdmin");
            db.Users.RemoveRange(existingAdmins);
            db.SaveChanges();
            // Recreate the admin.
            var hasher = new PasswordHasher<User>();
            var user = new User { Username = "InitialAdmin", Role = "ADMIN", Active = true };
            user.Password = hasher.HashPassword(user, "InitialAdminPassword");
            // Add the admin and save the changes.
            db.Users.Add(user);
            db.SaveChanges();
        }

        app.Run();
    }
}


