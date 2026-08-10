namespace WebAppBackend.Data;

using Microsoft.Data.SqlClient;

public static class DbConnectionProvider
{
    public static string GetProjectName()
    {
        return Path.GetFileName(
            Directory.GetCurrentDirectory().TrimEnd(Path.DirectorySeparatorChar));
    }

    public static string GetDevelopmentDatabaseName()
    {
        return $"DevDb_{GetProjectName()}";
    }

    public static string BuildDevelopmentConnectionString(string baseConnectionString)
    {
        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = GetDevelopmentDatabaseName()
        };

        return builder.ConnectionString;
    }
}