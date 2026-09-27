using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace Mini_Request_Management_Portal.Data
{
    public static class DatabaseHelper
    {
        //  Connection string
        public static SQLiteConnection GetConnection()
        {
            string connectionString = ConfigurationManager
                .ConnectionStrings["RequestPortalDb"]
                .ConnectionString;

            return new SQLiteConnection(connectionString);
        }


        /// Creates the database file, applies Schema.sql, and
        /// applies Seed.sql when running in development
        public static void InitialiseDatabase()
        {
            EnsureDatabaseFileExists();
            ApplyScript("Mini_Request_Management_Portal.Database.Schema.sql");
        }

        //  Private helpers

        private static void EnsureDatabaseFileExists()
        {
            string dbPath = ResolveDatabasePath();
            string directory = Path.GetDirectoryName(dbPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }
        }

        /// Reads and executes a SQL script that is embedded as a
        /// resource inside the assembly.  Each statement is separated
        /// by a blank line so the file stays readable
        private static void ApplyScript(string resourceName)
        {
            string sql = ReadEmbeddedResource(resourceName);

            using (SQLiteConnection connection = GetConnection())
            {
                connection.Open();

                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    // Remove comment lines before splitting
                    var lines = sql.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var cleanLines = new System.Text.StringBuilder();

                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();

                        // Skip full-line comments
                        if (trimmedLine.StartsWith("--"))
                            continue;

                        // Remove inline comments
                        int commentIndex = trimmedLine.IndexOf("--");
                        if (commentIndex >= 0)
                            cleanLines.AppendLine(trimmedLine.Substring(0, commentIndex));
                        else
                            cleanLines.AppendLine(trimmedLine);
                    }

                    // Split on semicolons to get individual statements
                    string[] statements = cleanLines.ToString().Split(
                        new[] { ";" },
                        StringSplitOptions.RemoveEmptyEntries
                    );

                    foreach (string statement in statements)
                    {
                        string trimmed = statement.Trim();

                        if (string.IsNullOrWhiteSpace(trimmed))
                            continue;

                        using (SQLiteCommand command = new SQLiteCommand(trimmed, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        /// Reads a SQL file that has been added to the project with
        private static string ReadEmbeddedResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException(
                        $"Embedded resource '{resourceName}' not found. " +
                        "Ensure the SQL file is included with Build Action = Embedded Resource.");
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static string ResolveDatabasePath()
        {
            string connectionString = ConfigurationManager
                .ConnectionStrings["RequestPortalDb"]
                .ConnectionString;

            SQLiteConnectionStringBuilder builder =
                new SQLiteConnectionStringBuilder(connectionString);

            string dataSource = builder.DataSource;

            if (dataSource.Contains("|DataDirectory|"))
            {
                string dataDir = AppDomain.CurrentDomain.GetData("DataDirectory") as string
                    ?? AppDomain.CurrentDomain.BaseDirectory;

                dataSource = dataSource.Replace("|DataDirectory|", dataDir);
            }

            return Path.GetFullPath(dataSource);
        }
    }
}
