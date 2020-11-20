using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDBMigrations;
using System;
using System.Reflection;
using Vc.DAL.Mongo.Repositories;

namespace Mongo.Migration.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            Console.WriteLine("Migrating!");

            Assembly assemblyWithMigrations = typeof(AbstractRepository).Assembly;

            string connectionString = config.GetConnectionString("mongodb");
            string databaseName = config.GetValue<string>("database");
            new MigrationEngine().UseDatabase(connectionString, databaseName)
                .UseAssembly(assemblyWithMigrations) //Required//Required to use specific db.UseAssembly(assemblyWithMigrations) //Required
                .UseSchemeValidation(false) //Optional true or false
                .Run(); // Execution call. Might be called without targetVersion, in that case, the engine will choose the latest available version.

        }
    }
}
