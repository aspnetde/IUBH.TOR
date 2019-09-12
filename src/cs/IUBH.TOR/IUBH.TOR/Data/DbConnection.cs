using System;
using System.Diagnostics;
using System.IO;
using IUBH.TOR.Modules.Courses.Domain;
using SQLite;
using Xamarin.Forms.Internals;

namespace IUBH.TOR.Data
{
    internal class DbConnection : IDbConnection
    {
        private SQLiteConnection _connection;

        private readonly object _lock = new object();

        private static readonly Lazy<DbConnection> s_instance =
            new Lazy<DbConnection>(() => new DbConnection());

        public static DbConnection Instance => s_instance.Value;

        /// <summary>
        /// Provides a connection to the local database. If the connection is
        /// not open yet, Initialize() is being called.
        /// </summary>
        public SQLiteConnection Connection
        {
            get
            {
                lock (_lock)
                {
                    if (_connection == null)
                    {
                        Initialize();
                    }

                    return _connection;
                }
            }
        }

        /// <summary>
        /// Initializes the connection: If necessary, the whole database file is
        /// being created including the Course table.
        /// </summary>
        public void Initialize()
        {
            lock (_lock)
            {
                _connection = CreateNewConnection();
                _connection.CreateTable<Course>();

                Debug.WriteLine($"Database Path: {_connection.DatabasePath}");
            }
        }

        /// <summary>
        /// Closes and disposes the database connection and deletes all SQLite files.
        /// </summary>
        public void Nuke()
        {
            lock (_lock)
            {
                string databasePath = _connection.DatabasePath;

                _connection.Close();
                _connection.Dispose();
                _connection = null;

                // Delete all database files, including the WAL etc.
                // ReSharper disable once PossibleNullReferenceException
                new FileInfo(databasePath).Directory.GetFiles().ForEach(file => file.Delete());
            }
        }

        private static SQLiteConnection CreateNewConnection()
        {
            var databasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "iubh-tor.db"
            );

            return new SQLiteConnection(databasePath);
        }
    }
}
