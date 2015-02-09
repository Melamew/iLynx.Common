using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Common;
using iLynx.Common.DataAccess;
using NHibernate.Linq;

namespace iLynx.Chatter.NHibernateModule
{
    /// <summary>
    /// NHibernateAdapter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NHibernateAdapter<T> : IDataAdapter<T> where T : class, IEntity
    {
        private readonly ISessionScoper sessionScoper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateAdapter{T}" /> class.
        /// </summary>
        /// <param name="sessionScoper">The session scoper.</param>
        public NHibernateAdapter(ISessionScoper sessionScoper)
        {
            sessionScoper.Guard("sessionScoper");
            this.sessionScoper = sessionScoper;
        }

        /// <summary>
        /// Stores the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Save(T value)
        {
            Transact(() => sessionScoper.GetSession().Save(value), true);
            //sessionScoper.GetSession().Insert(value);
        }

        /// <summary>
        /// Saves the or update.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SaveOrUpdate(T value)
        {
            Transact(() => sessionScoper.GetSession().SaveOrUpdate(value), true);
            //sessionScoper.GetSession().Insert(value);
        }

        public IEnumerable<T> DistinctBy<TK>(Expression<Func<T, TK>> keyExpression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Delete(T value)
        {
            Transact(() => sessionScoper.GetSession().Delete(value), true);
            //sessionScoper.GetSession().Delete(value);
        }

        /// <summary>
        /// Gets the first.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public T GetFirst(Expression<Func<T, bool>> predicate)
        {
            return sessionScoper.GetSession().Query<T>().FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets the first by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public T GetFirstById(object id)
        {
            if (!(id is Guid))
            {
                return default(T);
            }
            return sessionScoper.GetSession().Query<T>().FirstOrDefault(x => x.UniqueId == (Guid)id);
        }

        /// <summary>
        /// Transacts the specified a.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="flushAfter">if set to <c>true</c> [flush after].</param>
        public void Transact(Action a, bool flushAfter)
        {
            var session = sessionScoper.GetSession();
            using (var transaction = session.BeginTransaction())
            {
                a();
                transaction.Commit();
            }
            //if (!flushAfter) return;
            //sessionScoper.GetSession().Flush();
            //sessionScoper.GetSession().Clear();
        }

        /// <summary>
        /// Creates the index.
        /// </summary>
        /// <param name="fields">The fields.</param>
        public void CreateIndex(params string[] fields)
        {
        }

        /// <summary>
        /// Bulks the insert.
        /// </summary>
        /// <param name="data">The data.</param>
        public void BulkInsert(IEnumerable<T> data)
        {
            var session = sessionScoper.GetSession();
            using (var transaction = session.BeginTransaction())
            {
                foreach (var item in data)
                    session.SaveOrUpdate(item);
                transaction.Commit();
            }
            session.Flush();
        }

        /// <summary>
        /// Gets the first.
        /// </summary>
        /// <returns></returns>
        public T GetFirst()
        {
            return sessionScoper.GetSession().Query<T>().Take(1).FirstOrDefault();
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetAll()
        {
            return sessionScoper.GetSession().Query<T>().AsEnumerable();
        }

        /// <summary>
        /// Queries this instance.
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> Query()
        {
            return sessionScoper.GetSession().Query<T>();
        }
    }
}
