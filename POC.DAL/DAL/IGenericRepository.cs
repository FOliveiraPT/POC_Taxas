using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POC.DAL
{
    public interface IGenericRepository<T> : IDisposable where T : class
    {
        IQueryable<T> Fetch();
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Func<T, bool> predicate);
        T Single(Func<T, bool> predicate);
        T First(Func<T, bool> predicate);
        void Add(T entity, bool saveAction = true);
        void Add(List<T> list);
        void Update(T entity, List<string> listProperties);
        void Delete(T entity);
        void Attach(T entity);
        void SaveChanges();
        void SaveChanges(System.Data.Objects.SaveOptions options);
    }
}
