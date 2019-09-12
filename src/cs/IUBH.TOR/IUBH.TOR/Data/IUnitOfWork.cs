namespace IUBH.TOR.Data
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Opens a Unit of Work Session. Within that session data can
        /// be added, updated, or removed from the database.
        /// </summary>
        IUnitOfWorkSession OpenSession();
    }
}
