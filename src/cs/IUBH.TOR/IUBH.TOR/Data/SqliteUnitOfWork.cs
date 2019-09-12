namespace IUBH.TOR.Data
{
    public class SqliteUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Opens a Unit of Work Session. Within that session data can
        /// be added, updated, or removed from the database.
        /// </summary>
        public IUnitOfWorkSession OpenSession()
            => new SqliteUnitOfWorkSession(DbConnection.Instance.Connection);
    }
}
