using API.Hubs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Text;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            builder.Host.UseSerilog();
            #endregion

            #region ServiceRegistration
            builder.Services.AddControllers();
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequest>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSignalR();
            builder.Services.AddSwaggerGen(setup =>
            {
                // 'SecurityScheme' to use JWT Authentication
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "JWT Bearer token",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });

                setup.EnableAnnotations();
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                var connectionString = environment == "Development"
                    ? builder.Configuration.GetConnectionString("Default")
                    : $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST")};Port={Environment.GetEnvironmentVariable("POSTGRES_PORT")};Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")}";

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException($"Connection string for environment '{environment}' is not configured.");
                }

                if (environment == "Development")
                {
                    options.UseSqlite(connectionString);
                }
                else
                {
                    options.UseNpgsql(connectionString);
                }
            });

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Register JWT token generator
            builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // Register repositories
            builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
            builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            builder.Services.AddScoped<IBetRepository, BetRepository>();
            builder.Services.AddScoped<IGameRepository, GameRepository>();
            builder.Services.AddScoped<IWalletRepository, WalletRepository>();
            builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

            // Register services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPlayerService, PlayerService>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<IWalletTransactionService, WalletTransactionService>();
            builder.Services.AddScoped<IBetService, BetService>();
            builder.Services.AddScoped<IGameService, GameService>();
            builder.Services.AddScoped<IUserNotificationService, UserNotificationService<UserHub>>();
            builder.Services.AddScoped<IRandomService, RandomService>();
            builder.Services.AddScoped<ICurrencyService, CurrencyService>();
            #endregion

            #region Authentication
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                        )
                    };
                });
            #endregion

            #region AppPipeline
            var app = builder.Build();

            app.MapHub<UserHub>("/hubs/user");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
            #endregion
        }
    }
}
