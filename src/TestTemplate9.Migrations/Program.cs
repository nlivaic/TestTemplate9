using System;
using System.IO;
using DbUp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TestTemplate9.Migrations
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var connectionString = string.Empty;
            var dbUser = string.Empty;
            var dbPassword = string.Empty;
            var scriptsPath = string.Empty;

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";
            Console.WriteLine($"Environment: {env}.");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            InitializeParameters();
            var connectionStringBuilderTestTemplate9 = new SqlConnectionStringBuilder(connectionString);
            if (env.Equals("Development"))
            {
                connectionStringBuilderTestTemplate9.UserID = dbUser;
                connectionStringBuilderTestTemplate9.Password = dbPassword;
            }
            else
            {
                connectionStringBuilderTestTemplate9.UserID = dbUser;
                connectionStringBuilderTestTemplate9.Password = dbPassword;
                connectionStringBuilderTestTemplate9.Authentication = SqlAuthenticationMethod.ActiveDirectoryServicePrincipal;
            }
            Console.WriteLine("------------- " + connectionStringBuilderTestTemplate9.ConnectionString);
            var upgraderTestTemplate9 =
                DeployChanges.To
                    .SqlDatabase(connectionStringBuilderTestTemplate9.ConnectionString)
                    .WithScriptsFromFileSystem(
                        !string.IsNullOrWhiteSpace(scriptsPath)
                                ? Path.Combine(scriptsPath, "TestTemplate9Scripts")
                            : Path.Combine(Environment.CurrentDirectory, "TestTemplate9Scripts"))
                    .LogToConsole()
                    .Build();
            Console.WriteLine($"Now upgrading TestTemplate9.");
            var resultTestTemplate9 = upgraderTestTemplate9.PerformUpgrade();

            if (!resultTestTemplate9.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"TestTemplate9 upgrade error: {resultTestTemplate9.Error}");
                Console.ResetColor();
                return -1;
            }

            // Uncomment the below sections if you also have an Identity Server project in the solution.
            /*
            var connectionStringTestTemplate9Identity = string.IsNullOrWhiteSpace(args.FirstOrDefault())
                ? config["ConnectionStrings:TestTemplate9IdentityDb"]
                : args.FirstOrDefault();

            var upgraderTestTemplate9Identity =
                DeployChanges.To
                    .SqlDatabase(connectionStringTestTemplate9Identity)
                    .WithScriptsFromFileSystem(
                        scriptsPath != null
                            ? Path.Combine(scriptsPath, "TestTemplate9IdentityScripts")
                            : Path.Combine(Environment.CurrentDirectory, "TestTemplate9IdentityScripts"))
                    .LogToConsole()
                    .Build();
            Console.WriteLine($"Now upgrading TestTemplate9 Identity.");
            if (env != "Development")
            {
                upgraderTestTemplate9Identity.MarkAsExecuted("0004_InitialData.sql");
                Console.WriteLine($"Skipping 0004_InitialData.sql since we are not in Development environment.");
                upgraderTestTemplate9Identity.MarkAsExecuted("0005_Initial_Configuration_Data.sql");
                Console.WriteLine($"Skipping 0005_Initial_Configuration_Data.sql since we are not in Development environment.");
            }
            var resultTestTemplate9Identity = upgraderTestTemplate9Identity.PerformUpgrade();

            if (!resultTestTemplate9Identity.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"TestTemplate9 Identity upgrade error: {resultTestTemplate9Identity.Error}");
                Console.ResetColor();
                return -1;
            }
            */

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;

            void InitializeParameters()
            {
                Console.WriteLine("====================== ARGS START ======================");          // xx
                Console.WriteLine($"args.Length: {args.Length}");
                foreach (var arg in args)                                                               // xx
                {                                                                                       // xx
                    Console.WriteLine(arg);                                                             // xx
                }                                                                                       // xx
                Console.WriteLine("====================== ARGS END ======================");            // xx

                // Local database
                if (args.Length == 0)
                {
                    connectionString = config["ConnectionStrings:TestTemplate9Db_Migrations_Connection"];
                    dbUser = config["DB_USER"];
                    dbPassword = config["DB_PASSWORD"];
                }
                // Remote database
                else if (args.Length == 4)
                {
                    Console.WriteLine("====================== ARGS INDIVIDUAL START ======================");
                    connectionString = args[0];
                    dbUser = args[1];
                    dbPassword = args[2];
                    scriptsPath = args[3];
                    Console.WriteLine(args[0]);
                    Console.WriteLine(args[1]);
                    Console.WriteLine(args[2]);
                    Console.WriteLine(args[3]);
                    Console.WriteLine("====================== ARGS INDIVIDUAL END ======================");

                }
            }
        }
    }
}
