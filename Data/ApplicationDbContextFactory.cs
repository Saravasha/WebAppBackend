using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebAppBackend.Data;

public class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {

        var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        var connectionString = !string.IsNullOrEmpty(envConnectionString)
            ? envConnectionString
            : new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build()
                .GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection String Error.");

        Console.WriteLine($"🗄️ EF migration database: {new SqlConnectionStringBuilder(connectionString).InitialCatalog}");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);


        return new ApplicationDbContext(optionsBuilder.Options);
    }
}