using SQLite;

namespace IUBH.TOR.Data
{
    public interface IDbConnection
    {
        /// <summary>
        /// Provides a connection to the local database.
        /// </summary>
        SQLiteConnection Connection { get; }
        
        /// <summary>
        /// Initializes the local database so it can be used to read from and write to.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Closes and deletes the local database completely. There is no undo!
        /// </summary>
        void Nuke();
    }
}
