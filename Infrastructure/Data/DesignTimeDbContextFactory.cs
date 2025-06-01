using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            Console.WriteLine("DesignTimeDbContextFactory is being used.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var connectionString = environment == "Development"
                ? configuration.GetConnectionString("Default")
                : configuration.GetConnectionString("Production");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Connection string for environment '{environment}' is not configured.");
            }

            if (environment == "Development")
            {
                builder.UseSqlite(connectionString);
            }
            else
            {
                builder.UseNpgsql(connectionString);
            }

            return new AppDbContext(builder.Options);
        }
    }
}