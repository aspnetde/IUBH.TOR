using System;
using IUBH.TOR.Domain;

namespace IUBH.TOR.Data
{
    public interface IUnitOfWorkSession : IDisposable
    {
        /// <summary>
        /// Adds (inserts) an object to the database.
        /// Note: To persist that change, Commit() must be called.
        /// </summary>
        void Add(params object[] entities);
        
        /// <summary>
        /// Updates an object in the database.
        /// Note: To persist that change, Commit() must be called.
        /// </summary>
        void Update(params object[] entities);
        
        /// <summary>
        /// Removes an object from the database
        /// Note: To persist that change, Commit() must be called.
        /// </summary>
        void Remove(params object[] entities);

        /// <summary>
        /// Commits the database transaction, meaning that all changes
        /// regarding the in this session previously added, updated, and
        /// removed objects are now actually reflected in the database. 
        /// </summary>
        Result Commit();
    }
}
