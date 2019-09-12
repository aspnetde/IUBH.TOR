using System;
using System.Collections.Generic;
using IUBH.TOR.Domain;
using SQLite;

namespace IUBH.TOR.Data
{
    internal class SqliteUnitOfWorkSession : IUnitOfWorkSession
    {
        private readonly SQLiteConnection _connection;

        private readonly List<object> _entitiesToAdd = new List<object>();
        private readonly List<object> _entitiesToUpdate = new List<object>();
        private readonly List<object> _entitiesToRemove = new List<object>();

        private static readonly object s_lock = new object();

        public SqliteUnitOfWorkSession(SQLiteConnection connection) => _connection = connection;

        /// <summary>
        /// Adds (inserts) an object to the database.
        /// Note: To persist that change, Commit() must be called.
        /// </summary>
        public void Add(params object[] entities) => _entitiesToAdd.AddRange(entities);
        
        /// <summary>
        /// Updates an object in the database.
        /// Note: To persist that change, Commit() must be called.
        /// </summary>
        public void Update(params object[] entities) => _entitiesToUpdate.AddRange(entities);
        
        /// <summary>
        /// Removes an object from the database
        /// Note: To persist that change, Commit() must be called.
        /// </summary>
        public void Remove(params object[] entities) => _entitiesToRemove.AddRange(entities);

        /// <summary>
        /// Commits the database transaction, meaning that all changes
        /// regarding the in this session previously added, updated, and
        /// removed objects are now actually reflected in the database.
        ///
        /// When an error occurs the exception will be passed along in the
        /// Result. Otherwise the Result will be marked as successful. 
        /// </summary>
        public Result Commit()
        {
            lock (s_lock)
            {
                try
                {
                    _connection.BeginTransaction();

                    _entitiesToRemove.ForEach(entity => _connection.Delete(entity));
                    _entitiesToAdd.ForEach(entity => _connection.Insert(entity));
                    _entitiesToUpdate.ForEach(entity => _connection.Update(entity));

                    _connection.Commit();

                    return Result.Success;
                }
                catch (Exception e)
                {
                    _connection.Rollback();

                    return Result.WithException(e);
                }
            }
        }

        public void Dispose()
        {
            _entitiesToAdd.Clear();
            _entitiesToUpdate.Clear();
            _entitiesToRemove.Clear();
        }
    }
}
